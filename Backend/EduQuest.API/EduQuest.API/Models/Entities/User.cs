namespace EduQuest.API.Models.Entities
{
    public class User
    {

        public int UserID { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Password reset (both are null unless a reset has been requested)
        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetExpiresAt { get; set; }

        // Foreign Key
        public int RoleID { get; set; }

        // Navigation Property
        public Role? Role { get; set; }

    }
}