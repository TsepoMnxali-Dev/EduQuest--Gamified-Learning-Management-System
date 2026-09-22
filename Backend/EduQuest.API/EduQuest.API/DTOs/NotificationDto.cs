namespace EduQuest.API.DTOs
{
    public class NotificationDto
    {
        public int NotificationID { get; set; }
        public required string Title { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime DateSent { get; set; }
        public int LearnerID { get; set; }
    }
}