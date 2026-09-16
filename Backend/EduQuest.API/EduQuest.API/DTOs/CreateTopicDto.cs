using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateTopicDto
    {
        [Range(1, int.MaxValue)]
        public int SubjectID { get; set; }

        [Required]
        [StringLength(200)]
        public required string TopicName { get; set; }

        [Required]
        [StringLength(20)]
        public required string GradeLevel { get; set; }
    }
}