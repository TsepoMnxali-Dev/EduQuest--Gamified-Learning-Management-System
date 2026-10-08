
namespace EduQuest.API.DTOs
{
    public class SubmitQuizAttemptDto
    {
        public List<SubmitQuizAnswerDto> Answers { get; set; } = new();
    }

    public class SubmitQuizAnswerDto
    {
        public int QuizAttemptQuestionID { get; set; }

        public int QuizOptionID { get; set; }
    }
}

