namespace EduQuest.API.Models.Entities
{
    public class Learner
    {
        public int LearnerID { get; set; }

        public int UserID { get; set; }

        public int GradeID { get; set; }
        public Grade Grade { get; set; }

        public int SchoolID { get; set; }
        public School School { get; set; }
        public User? User { get; set; }

        public ICollection<LearnerSubject> LearnerSubjects { get; set; }
           = new List<LearnerSubject>();
        public ICollection<LearnerAchievement> LearnerAchievement { get; set; } = new List<LearnerAchievement>();
        public ICollection<CompetitionLearner> CompetitionLearners { get; set; } = new List<CompetitionLearner>();
        public ICollection<Notification> Notification { get; set; } = new List<Notification>();
        public ICollection<LeaderBoard> LeaderBoard { get; set; } = new List<LeaderBoard>();
        public ICollection<QuizAttempt> QuizAttempt { get; set; } = new List<QuizAttempt>();

    }
}
