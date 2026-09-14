using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateSubjectDto
    {
        [Required]
        [StringLength(100)]
        public required string SubjectName { get; set; }

       
    }
}
