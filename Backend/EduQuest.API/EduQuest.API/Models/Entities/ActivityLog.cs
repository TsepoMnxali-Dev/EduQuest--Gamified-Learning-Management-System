namespace EduQuest.API.Models.Entities
{
    public class ActivityLog
    {
        public int ActivityLogID { get; set; }

        public int UserID { get; set; }

        public string Action { get; set; }

        public DateTime DateTime { get; set; }

        public string IPAddress { get; set; }

        public User User { get; set; }
    }
}
