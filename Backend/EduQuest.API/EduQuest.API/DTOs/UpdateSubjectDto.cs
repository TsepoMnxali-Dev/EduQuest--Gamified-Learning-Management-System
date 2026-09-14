using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class UpdateSubjectDto
    {
        [Required]
        [StringLength(100)]
        public required string SubjectName { get; set; }

       
    }
}
