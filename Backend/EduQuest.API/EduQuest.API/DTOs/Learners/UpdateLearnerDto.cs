using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs.Learners
{
    public class UpdateLearnerDto
    {
        [Required]
        public int GradeID { get; set; }

        [Required]
        public int SchoolID { get; set; }
    }
}
