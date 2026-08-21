namespace EduQuest.API.Models.Entities
{
    public class LearnerSubject
    {
        public int LearnerID { get; set; }
        public int SubjectID { get; set; }
        public string GradeLevel { get; set; }
        public Learner Learner { get; set; }
        public Subject Subject { get; set; }

    }
}
