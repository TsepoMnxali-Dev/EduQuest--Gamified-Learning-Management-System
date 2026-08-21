using EduQuest.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Data
{
    public class EduQuestDBContext: DbContext
    {
        public EduQuestDBContext(DbContextOptions<DbContext> options)
         : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Learner> Learners { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }
    }
}

