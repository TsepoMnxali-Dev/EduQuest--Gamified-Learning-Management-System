namespace EduQuest.API.Models.Entities
{
    public class Prize
    {
        public int PrizeID { get; set; }
        public required string PrizeName { get; set; }
        public required string PrizeDescription { get; set; }
        public double Value { get; set; }

        public int CompetitionID { get; set; }
        public Competition Competition { get; set; }
    }
}
