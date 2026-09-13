namespace EduQuest.API.Models.Entities
{
    public class Achievement
    {
        public int AchievementID { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int PointsRequired { get; set; }
        public required string BadgeImage { get; set; }

        public ICollection<LearnerAchievement> LearnerAchievements { get; set; } = new List<LearnerAchievement>();
    }
}
