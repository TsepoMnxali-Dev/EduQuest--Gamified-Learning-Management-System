namespace EduQuest.API.Models.Entities
{
    public class Topic
    {
        public int TopicID { get; set; }
        public int GradeLevel { get; set; } //we might write this as a string too.



        public ICollection<Quiz>? Quizes { get; set; }

        public int SubjectID {  get; set; }

        public Subject? Subject { get; set; }
    }
}
