using EduQuest.API.Data;
using EduQuest.API.DTOs;
using EduQuest.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace EduQuest.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public NotificationsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications()
        {
            var notifications = await _context.Notifications
                .Select(notification => new NotificationDto
                {
                    NotificationID = notification.NotificationID,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    DateSent = notification.DateSent,
                    LearnerID = notification.LeanerID
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/notifications/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDto>> GetNotification(int id)
        {
            var notification = await _context.Notifications
                .Where(notification => notification.NotificationID == id)
                .Select(notification => new NotificationDto
                {
                    NotificationID = notification.NotificationID,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    DateSent = notification.DateSent,
                    LearnerID = notification.LeanerID
                })
                .FirstOrDefaultAsync();

            if (notification == null)
            {
                return NotFound();
            }

            return Ok(notification);
        }


        [Authorize(Roles = "Admin")]

        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification(CreateNotificationDto dto)
        {
            // Normalize the input
            var title = dto.Title.Trim();
            var message = dto.Message?.Trim();

            // Layer 2: Check that the referenced Learner actually exists
            var learnerExists = await _context.Learners
                .AnyAsync(learner => learner.LearnerID == dto.LearnerID);

            if (!learnerExists)
            {
                return BadRequest("The specified LearnerID does not exist.");
            }

            var notificationEntity = new Notification
            {
                Title = title,
                Message = message,
                IsRead = false,
                DateSent = dto.DateSent,
                LeanerID = dto.LearnerID
            };

            _context.Notifications.Add(notificationEntity);
            await _context.SaveChangesAsync();

            var result = new NotificationDto
            {
                NotificationID = notificationEntity.NotificationID,
                Title = notificationEntity.Title,
                Message = notificationEntity.Message,
                IsRead = notificationEntity.IsRead,
                DateSent = notificationEntity.DateSent,
                LearnerID = notificationEntity.LeanerID
            };

            return CreatedAtAction(
                nameof(GetNotification),
                new { id = notificationEntity.NotificationID },
                result
            );
        }


        [Authorize(Roles = "Admin")]

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNotification(
            int id,
            UpdateNotificationDto dto)
        {
            var notificationEntity = await _context.Notifications.FindAsync(id);

            if (notificationEntity == null)
            {
                return NotFound();
            }

            // Normalize the input
            var title = dto.Title.Trim();
            var message = dto.Message?.Trim();

            // Layer 2: Check that the referenced Learner actually exists
            var learnerExists = await _context.Learners
                .AnyAsync(learner => learner.LearnerID == dto.LearnerID);

            if (!learnerExists)
            {
                return BadRequest("The specified LearnerID does not exist.");
            }

            notificationEntity.Title = title;
            notificationEntity.Message = message;
            notificationEntity.IsRead = dto.IsRead;
            notificationEntity.DateSent = dto.DateSent;
            notificationEntity.LeanerID = dto.LearnerID;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize(Roles = "Admin")]

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var notificationEntity = await _context.Notifications.FindAsync(id);

            if (notificationEntity == null)
            {
                return NotFound();
            }

            _context.Notifications.Remove(notificationEntity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/notifications/{id}/read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notificationEntity = await _context.Notifications.FindAsync(id);

            if (notificationEntity == null)
            {
                return NotFound();
            }

            notificationEntity.IsRead = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}