namespace EduQuest.API.DTOs
{
    public class AchievementBadgeDto
    {
        public int AchievementID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BadgeImage { get; set; } = string.Empty;
        public int PointsRequired { get; set; }
    }
}
