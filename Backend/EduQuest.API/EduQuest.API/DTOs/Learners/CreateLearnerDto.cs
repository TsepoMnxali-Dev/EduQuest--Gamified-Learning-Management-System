using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs.Learners
{
    public class CreateLearnerDto
    {
        [Required]
        public int UserID { get; set; }

        [Required]
        public int GradeID { get; set; }

        [Required]
        public int SchoolID { get; set; }
    }
}
