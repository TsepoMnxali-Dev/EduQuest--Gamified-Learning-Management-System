namespace EduQuest.API.Models.Entities
{
    public class Learner
    {
        public int LearnerID { get; set; }

        public int UserID { get; set; }

        public int GradeID { get; set; }

        public string SchoolName { get; set; }

        public string Province { get; set; }

        public User User { get; set; }
    }
}
