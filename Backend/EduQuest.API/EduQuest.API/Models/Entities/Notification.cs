namespace EduQuest.API.Models.Entities
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public required string Title {  get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public required string DateSent { get; set; }

        public int LeanerID { get; set; }
        public Learner? Learner { get; set; }
    }
}
