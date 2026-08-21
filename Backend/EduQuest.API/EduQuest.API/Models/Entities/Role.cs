namespace EduQuest.API.Models.Entities
{
    public class Role
    {
        public int RoleID { get; set; }

        public string RoleName { get; set; }
        
        public required string RoleName { get; set; }



        public ICollection<User>? Users { get; set; }

    }
}
