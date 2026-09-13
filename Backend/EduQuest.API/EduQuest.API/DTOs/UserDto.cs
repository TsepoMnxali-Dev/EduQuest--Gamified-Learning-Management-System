namespace EduQuest.API.DTOs.Users
{
    public class UserDto
    {
        public int UserID { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}