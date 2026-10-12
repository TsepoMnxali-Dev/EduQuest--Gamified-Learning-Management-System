using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.Models.Entities
{
    public class SupportRequest
    {
        public int SupportRequestID { get; set; }

        public int UserID { get; set; }
        public User? User { get; set; }

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(150)]
        public string Subject { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        // "Open", "In Progress" or "Resolved"
        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Written by an admin; shown to the learner.
        [MaxLength(2000)]
        public string? AdminReply { get; set; }
        public DateTime? DateReplied { get; set; }
    }
}