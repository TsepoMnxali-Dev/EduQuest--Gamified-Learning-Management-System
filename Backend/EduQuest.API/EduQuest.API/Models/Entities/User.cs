namespace EduQuest.API.Models.Entities
{
    public class User
    {
        public int UserID { get; set; }

        public int RoleID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public Role Role { get; set; }
    }
}
