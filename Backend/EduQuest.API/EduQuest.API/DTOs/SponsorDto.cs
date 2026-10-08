namespace EduQuest.API.DTOs
{
    public class SponsorDto
    {
        public int SponsorID { get; set; }
        public required string CompanyName { get; set; }
        public required string ContactEmail { get; set; }
    }
}