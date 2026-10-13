using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs.Auth
{
    public class ResetPasswordDto
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(8), StringLength(100)]
        public string NewPassword { get; set; } = string.Empty;
    }
}