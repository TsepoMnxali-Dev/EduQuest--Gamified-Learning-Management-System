namespace EduQuest.API.DTOs
{
    public class CompetitionDto
    {
        public int CompetitionID { get; set; }
        public string? SponsorName { get; set; }
        public required string StartDate { get; set; }
        public required string EndDate { get; set; }
        public required string Description { get; set; }
        public int SponsorID { get; set; }
    }
}