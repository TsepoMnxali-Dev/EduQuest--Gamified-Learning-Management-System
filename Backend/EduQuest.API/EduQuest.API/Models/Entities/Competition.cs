namespace EduQuest.API.Models.Entities
{
    public class Competition
    {
        public int CompetitionID { get; set; }
        public string? SponsorName { get; set; }

        public required string StartDate { get; set; }
        public required string EndDate { get; set; }
        public required string Description { get; set; }

        public ICollection<Prize>? Prizes { get; set; }
        public ICollection<CompetitionLearner> CompetitionLearners { get; set; } = new List<CompetitionLearner>();


        // Foreign Key from Sponsor
        public int SponsorID { get; set; }
        public Sponsor? Sponsor { get; set; }
    }
}
