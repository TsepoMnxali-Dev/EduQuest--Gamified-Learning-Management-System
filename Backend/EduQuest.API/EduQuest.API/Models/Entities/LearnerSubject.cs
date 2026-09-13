namespace EduQuest.API.Models.Entities
{
    public class LearnerSubject
    {
        public int LearnerSubjectID { get; set; }

        public int LearnerID { get; set; }
        public Learner Learner { get; set; }

        public int SubjectID { get; set; }
        public Subject Subject { get; set; }

        public required string GradeLevel { get; set; }
    }
}