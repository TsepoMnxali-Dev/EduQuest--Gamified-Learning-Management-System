namespace EduQuest.API.Models.Entities
{
    public class School
    {
        public int SchoolID { get; set; }

        public required string SchoolName { get; set; }

        public int ProvinceID { get; set; }
        public Province Province { get; set; }

        public ICollection<Learner> Learners { get; set; }
            = new List<Learner>();
    }
}