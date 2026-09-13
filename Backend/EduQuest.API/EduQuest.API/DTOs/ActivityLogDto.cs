namespace EduQuest.API.DTOs
{
    public class ActivityLogDto
    {
        public int ActivityLogID { get; set; }
        public int UserID { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string IPAddress { get; set; } = string.Empty;
    }
}