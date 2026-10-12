using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs.Auth
{
    public class RegisterLearnerDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8), StringLength(100)]
        public string Password { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int GradeID { get; set; }

        [Range(1, int.MaxValue)]
        public int SchoolID { get; set; }

        [Required, MinLength(1)]
        public List<int> SubjectIDs { get; set; } = new();
    }
}