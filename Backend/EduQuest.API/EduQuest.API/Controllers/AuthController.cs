
using EduQuest.API.Data;
using EduQuest.API.DTOs.Auth;
using EduQuest.API.Models.Entities;
using EduQuest.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly TokenService _tokenService;

        public AuthController(
            ApplicationDBContext context,
            TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // ==========================================
        // EXISTING: BASIC LEARNER REGISTRATION
        // ==========================================

        [HttpPost("register")]
        public async Task<ActionResult> Register(
            RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(
                u => u.Email == email))
            {
                return Conflict(
                    "An account with this email already exists.");
            }

            var learnerRole = await _context.Roles
                .FirstOrDefaultAsync(
                    r => r.RoleName == "Learner");

            if (learnerRole == null)
            {
                return StatusCode(
                    500,
                    "Learner role has not been configured.");
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = email,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.Password),

                RoleID = learnerRole.RoleID,
                IsActive = true
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.UserID,
                user.Email
            });
        }

        // ==========================================
        // NEW: COMPLETE LEARNER REGISTRATION
        // POST: api/auth/register-learner
        // ==========================================

        [HttpPost("register-learner")]
        public async Task<ActionResult> RegisterLearner(
            RegisterLearnerDto dto)
        {
            // NEW: Validate incoming request.
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new
                {
                    message = "Please complete all account details."
                });
            }

            var email = dto.Email.Trim().ToLowerInvariant();

            // NEW: Prevent duplicate email addresses.
            if (await _context.Users.AnyAsync(
                u => u.Email == email))
            {
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });
            }

            // NEW: Find the Learner role.
            var learnerRole = await _context.Roles
                .FirstOrDefaultAsync(
                    r => r.RoleName == "Learner");

            if (learnerRole == null)
            {
                return StatusCode(500, new
                {
                    message = "Learner role is not configured."
                });
            }

            // NEW: Validate the selected grade.
            var gradeExists = await _context.Grades
                .AnyAsync(g => g.GradeID == dto.GradeID);

            if (!gradeExists)
            {
                return BadRequest(new
                {
                    message = "The selected grade does not exist."
                });
            }

            // NEW: Validate the selected school.
            var schoolExists = await _context.Schools
                .AnyAsync(s => s.SchoolID == dto.SchoolID);

            if (!schoolExists)
            {
                return BadRequest(new
                {
                    message = "The selected school does not exist."
                });
            }

            // NEW: Validate subject selections.
            if (dto.SubjectIDs == null ||
                dto.SubjectIDs.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Please select at least one subject."
                });
            }

            var subjectIds = dto.SubjectIDs.Distinct().ToList();

            if (subjectIds.Any(id => id <= 0))
            {
                return BadRequest(new
                {
                    message = "One or more selected subjects are invalid."
                });
            }

            var validSubjectCount = await _context.Subjects
                .CountAsync(s => subjectIds.Contains(s.SubjectID));

            if (validSubjectCount != subjectIds.Count)
            {
                return BadRequest(new
                {
                    message = "One or more selected subjects do not exist."
                });
            }

            // NEW: Ensure subjects belong to the
            // selected grade.
            var validGradeSubjectCount =
                await _context.GradeSubjects.CountAsync(
                    gs => gs.GradeID == dto.GradeID &&
                          subjectIds.Contains(gs.SubjectID));

            if (validGradeSubjectCount != subjectIds.Count)
            {
                return BadRequest(new
                {
                    message =
                        "One or more subjects are not available for the selected grade."
                });
            }

            // NEW: Transaction ensures that user,
            // learner profile and subjects are
            // saved together or not at all.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ==================================
                // CREATE USER ACCOUNT
                // ==================================

                var user = new User
                {
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    Email = email,

                    PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            dto.Password),

                    RoleID = learnerRole.RoleID,
                    Role = learnerRole,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // ==================================
                // CREATE LEARNER PROFILE
                // ==================================

                var learner = new Learner
                {
                    UserID = user.UserID,
                    GradeID = dto.GradeID,
                    SchoolID = dto.SchoolID
                };

                _context.Learners.Add(learner);
                await _context.SaveChangesAsync();

                // ==================================
                // SAVE SELECTED SUBJECTS
                // ==================================

                foreach (var subjectId in subjectIds)
                {
                    var learnerSubject = new LearnerSubject
                    {
                        LearnerID = learner.LearnerID,
                        SubjectID = subjectId
                    };

                    _context.learnerSubjects.Add(learnerSubject);
                }

                await _context.SaveChangesAsync();

                // ==================================
                // GENERATE AUTHENTICATION TOKEN
                // ==================================

                var token = _tokenService.CreateToken(user);

                await transaction.CommitAsync();

                // NEW: Same response structure as
                // the existing login endpoint.
                return Ok(new
                {
                    message = "Learner account created successfully.",

                    token = token,

                    user = new
                    {
                        userId = user.UserID,
                        learnerId = learner.LearnerID,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        email = user.Email,
                        role = learnerRole.RoleName
                    }
                });
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message =
                        "Unable to save your learner account. Please try again."
                });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message =
                        "An unexpected error occurred during registration."
                });
            }
        }

        // ==========================================
        // EXISTING: ADMIN AND LEARNER LOGIN
        // ==========================================

        [HttpPost("login")]
        public async Task<ActionResult> Login(
            LoginDto dto)
        {
            if (dto == null ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new
                {
                    message =
                        "Please enter your email and password."
                });
            }

            var email = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(
                    u => u.Email == email && u.IsActive);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.PasswordHash))
            {
                return Unauthorized(new
                {
                    message =
                        "Incorrect email or password. Please try again."
                });
            }

            if (user.Role == null ||
                string.IsNullOrWhiteSpace(
                    user.Role.RoleName))
            {
                return StatusCode(500, new
                {
                    message =
                        "Account role is not configured. Contact support."
                });
            }

            var token = _tokenService.CreateToken(user);

            return Ok(new
            {
                message = "Login successful.",

                token = token,

                user = new
                {
                    userId = user.UserID,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    role = user.Role.RoleName
                }
            });
        }
    }
}
