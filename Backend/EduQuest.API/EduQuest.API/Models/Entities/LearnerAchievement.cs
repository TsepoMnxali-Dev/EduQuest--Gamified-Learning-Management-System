namespace EduQuest.API.Models.Entities
{
    public class LearnerAchievement
    {
       
        public int LearnerAchievementID { get; set; }


        public int LearnerID { get; set; }
        public Learner? Learner { get; set; }

        public int AchievementID { get; set; }
        public Achievement? Achievement { get; set; }

    }
}
