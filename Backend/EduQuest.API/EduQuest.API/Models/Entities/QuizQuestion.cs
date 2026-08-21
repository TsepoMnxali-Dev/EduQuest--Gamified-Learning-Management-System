using System.Globalization;

namespace EduQuest.API.Models.Entities
{
    public class QuizQuestion
    {
        public int QuestionID { get; set; }

        public required string QuestionText { get; set; }
        public required string Explanation {  get; set; }
        public required string GeneratedByAI { get; set; }
        public required string ApprovedByAdmin {  get; set; }



        public int QuizID { get; set; }
        public Quiz? Quiz { get; set; }

        public ICollection<QuizOption>? QuizOptions { get; set; }


        public ICollection<QuizAttemptAnswer>? QuizAttemptAnswers { get; set; }



    }
}
