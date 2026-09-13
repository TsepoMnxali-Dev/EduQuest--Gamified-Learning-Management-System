namespace EduQuest.API.Models.Entities
{
    public class CompetitionLearner
    {


        public int CompetitionLearnerID { get; set; }
        public Competition? Competition { get; set; }


        public int LearnerID { get; set; }
        public Learner? Learner { get; set; }


        public int Score { get; set; }
        public int Position { get; set; }


       
        
       
    }
}
