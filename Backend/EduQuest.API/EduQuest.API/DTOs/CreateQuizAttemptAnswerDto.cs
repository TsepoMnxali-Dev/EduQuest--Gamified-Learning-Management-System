using System.ComponentModel.DataAnnotations;

namespace EduQuest.API.DTOs
{
    public class CreateQuizAttemptAnswerDto
    {
        [Required]
        public int AttemptID { get; set; }

        [Required]
        public int QuestionID { get; set; }

        [Required]
        public int QuizOptionID { get; set; }
    }
}