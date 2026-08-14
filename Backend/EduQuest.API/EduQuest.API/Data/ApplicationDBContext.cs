using EduQuest.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            
        }
        public DbSet<Role> Roles { get; set; }

    }
}
