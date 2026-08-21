namespace EduQuest.API.DTOs.Users
{
    public class UpdateUserDto
    {
        public int RoleID { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}
