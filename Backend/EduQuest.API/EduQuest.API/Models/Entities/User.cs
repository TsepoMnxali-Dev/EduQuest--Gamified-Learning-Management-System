using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuest.API.Models.Entities
{
    public class User
    {

        public int UserID { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash {  get; set; }
        public required string IsActive { get; set; }
        public string? DateCreated { get; set; }



        // Foreign Key
        public int RoleID { get; set; }

        // Navigation Property
        public Role? Role { get; set; }
        
    }
}
