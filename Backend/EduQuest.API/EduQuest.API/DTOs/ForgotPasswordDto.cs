using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;
    }
}