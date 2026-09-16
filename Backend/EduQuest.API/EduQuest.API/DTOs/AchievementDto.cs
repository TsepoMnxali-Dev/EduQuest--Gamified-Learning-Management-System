namespace EduQuest.API.DTOs
{
    public class AchievementDto
    {
        public int AchievementID { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int PointsRequired { get; set; }
        public required string BadgeImage { get; set; }
    }
}