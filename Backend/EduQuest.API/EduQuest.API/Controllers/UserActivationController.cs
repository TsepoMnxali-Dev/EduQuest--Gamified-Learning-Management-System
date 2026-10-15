using EduQuest.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduQuest.API.Controllers
{
    // Lets an admin switch a deactivated account back on.
    // (DELETE /api/users/{id} deactivates; this undoes it.)
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/users/{id:int}/activate")]
    public class UserActivationController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public UserActivationController(ApplicationDBContext context)
        {
            _context = context;
        }

        // POST: api/users/{id}/activate
        [HttpPost]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound("User not found.");

            // Already active: nothing to change.
            if (!user.IsActive)
            {
                user.IsActive = true;
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}