namespace EduQuest.API.Models.Entities
{
    public class Sponsor
    {
        public int SponsorID { get; set; }
        public required string CompanyName { get; set; }
        public required string ContactEmail { get; set; }


        
        public ICollection<Competition> Collections { get; set; } = new List<Competition>();

    }
}
