namespace EduQuest.API.DTOs
{
    public class QuizAttemptAnswerDto
    {
        public int QuizAttemptAnswerID { get; set; }
        public int AttemptID { get; set; }
        public int QuestionID { get; set; }
        public int QuizOptionID { get; set; }
    }
}