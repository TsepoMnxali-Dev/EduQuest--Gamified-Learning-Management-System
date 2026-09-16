using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateLearnerAchievementDto
    {
        [Required]
        public int AchievementID { get; set; }
    }
}