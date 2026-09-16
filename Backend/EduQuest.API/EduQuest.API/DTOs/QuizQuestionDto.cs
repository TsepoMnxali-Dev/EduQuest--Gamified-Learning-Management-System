namespace EduQuest.API.DTOs
{
    public class QuizQuestionDto
    {
        public int QuizQuestionID { get; set; }
        public required string QuestionText { get; set; }
        public required string Explanation { get; set; }
        public required string GeneratedByAI { get; set; }
        public required string ApprovedByAdmin { get; set; }
        public int QuizID { get; set; }
    }
}