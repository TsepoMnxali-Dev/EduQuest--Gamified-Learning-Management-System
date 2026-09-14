using EduQuest.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(
            ApplicationDBContext context,
            IConfiguration configuration)
        {
            // Find the Admin role by name
            var adminRole = await context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Admin");

            if (adminRole == null)
            {
                throw new InvalidOperationException(
                    "Admin role has not been configured.");
            }

            // Get Admin credentials from User Secrets
            var adminEmail = configuration["AdminSeed:Email"];
            var adminPassword = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Admin seed credentials have not been configured.");
            }

            // Check whether an Admin already exists
            var adminExists = await context.Users
                .AnyAsync(u => u.RoleID == adminRole.RoleID);

            if (adminExists)
            {
                return;
            }

            // Create the Admin
            var admin = new User
            {
                FirstName = "EduQuest",
                LastName = "Admin",
                Email = adminEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                IsActive = true,
                RoleID = adminRole.RoleID,
                DateCreated = DateTime.UtcNow
            };

            context.Users.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}