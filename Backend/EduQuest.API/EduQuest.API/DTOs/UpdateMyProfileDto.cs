using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs.Learners
{
    public class UpdateMyProfileDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public int GradeID { get; set; }

        [Required]
        public int SchoolID { get; set; }
    }
}
