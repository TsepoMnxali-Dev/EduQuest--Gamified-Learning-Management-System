using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateSupportRequestDto
    {
        [Required]
        public string Category { get; set; } = string.Empty;

        [Required, StringLength(150, MinimumLength = 3)]
        public string Subject { get; set; } = string.Empty;

        [Required, StringLength(2000, MinimumLength = 10)]
        public string Message { get; set; } = string.Empty;
    }

    public class SupportRequestDto
    {
        public int SupportRequestID { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public string? AdminReply { get; set; }
        public DateTime? DateReplied { get; set; }
    }

    // What an admin sees: the request plus who sent it.
    public class AdminSupportRequestDto : SupportRequestDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateSupportRequestDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? AdminReply { get; set; }
    }
}