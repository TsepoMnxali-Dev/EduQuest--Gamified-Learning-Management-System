using EduQuest.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduQuest.API.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
            
        }
       
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<CompetitionLearner> CompetitionLearners { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<GradeSubject> GradeSubjects { get; set; }
        public DbSet<LeaderBoard> LeaderBoards { get; set; }
        public DbSet<Learner> Learners { get; set; }
        public DbSet<LearnerAchievement> LearnerAchievements { get; set; }
        public DbSet<LearnerSubject> learnerSubjects { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Prize> Prizes { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<StudyMaterial> StudyMaterials { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ActivityLog> activityLogs { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<School> Schools { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GradeSubject>()
                .HasOne(gs => gs.Grade)
                .WithMany(g => g.GradeSubjects)
                .HasForeignKey(gs => gs.GradeID);

            modelBuilder.Entity<GradeSubject>()
                .HasOne(gs => gs.Subject)
                .WithMany(s => s.GradeSubjects)
                .HasForeignKey(gs => gs.SubjectID);

            modelBuilder.Entity<GradeSubject>()
                .HasIndex(gs => new { gs.GradeID, gs.SubjectID })
                .IsUnique();

            modelBuilder.Entity<LearnerSubject>()
           .HasIndex(ls => new { ls.LearnerID, ls.SubjectID })
           .IsUnique();

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleID = 1,
                    RoleName = "Learner"
                },
                new Role
                {
                    RoleID = 2,
                    RoleName = "Admin"
                },
                new Role
                {
                    RoleID = 3,
                    RoleName = "Sponsor"
    }
);

            modelBuilder.Entity<School>()
                .HasOne(school => school.Province)
                .WithMany(province => province.Schools)
                .HasForeignKey(school => school.ProvinceID);

            modelBuilder.Entity<Learner>()
                .HasOne(learner => learner.School)
                .WithMany(school => school.Learners)
                .HasForeignKey(learner => learner.SchoolID);

            modelBuilder.Entity<Topic>()
                .HasIndex(t => new
                {
                    t.SubjectID,
                    t.GradeLevel,
                    t.TopicName
                })
                .IsUnique();

            modelBuilder.Entity<Learner>()
                .HasOne(learner => learner.User)
                .WithOne()
                .HasForeignKey<Learner>(learner => learner.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<StudyMaterial>()
                .HasOne(sm => sm.Topic)
                .WithMany()
                .HasForeignKey(sm => sm.TopicID);

            modelBuilder.Entity<Province>().HasData(
                new Province { ProvinceID = 1, ProvinceName = "Eastern Cape" },
                new Province { ProvinceID = 2, ProvinceName = "Free State" },
                new Province { ProvinceID = 3, ProvinceName = "Gauteng" },
                new Province { ProvinceID = 4, ProvinceName = "KwaZulu-Natal" },
                new Province { ProvinceID = 5, ProvinceName = "Limpopo" },
                new Province { ProvinceID = 6, ProvinceName = "Mpumalanga" },
                new Province { ProvinceID = 7, ProvinceName = "Northern Cape" },
                new Province { ProvinceID = 8, ProvinceName = "North West" },
                new Province { ProvinceID = 9, ProvinceName = "Western Cape" }
            );

            modelBuilder.Entity<School>().HasData(
    // =========================
    // Eastern Cape - Province 1
    // =========================
    new School { SchoolID = 1, SchoolName = "AM Zantsi Senior Secondary School", ProvinceID = 1 },
    new School { SchoolID = 2, SchoolName = "Amabele Senior Secondary School", ProvinceID = 1 },
    new School { SchoolID = 3, SchoolName = "Emdemi Senior Secondary School", ProvinceID = 1 },
    new School { SchoolID = 4, SchoolName = "St Margaret Senior Secondary School", ProvinceID = 1 },
    new School { SchoolID = 5, SchoolName = "St Matthews High School", ProvinceID = 1 },

    // =========================
    // Free State - Province 2
    // =========================
    new School { SchoolID = 6, SchoolName = "Sehlabeng Secondary School", ProvinceID = 2 },
    new School { SchoolID = 7, SchoolName = "Sehunelo Secondary School", ProvinceID = 2 },
    new School { SchoolID = 8, SchoolName = "Selelekelela Secondary School", ProvinceID = 2 },
    new School { SchoolID = 9, SchoolName = "Seotlong A Secondary School", ProvinceID = 2 },
    new School { SchoolID = 10, SchoolName = "Teto Secondary School", ProvinceID = 2 },

    // =========================
    // Gauteng - Province 3
    // =========================
    new School { SchoolID = 11, SchoolName = "Adam Masebe Secondary School", ProvinceID = 3 },
    new School { SchoolID = 12, SchoolName = "Altmont Technical High School", ProvinceID = 3 },
    new School { SchoolID = 13, SchoolName = "Asser Maloka Secondary School", ProvinceID = 3 },
    new School { SchoolID = 14, SchoolName = "Bona Lesedi Secondary School", ProvinceID = 3 },
    new School { SchoolID = 15, SchoolName = "Cosmo City Secondary School", ProvinceID = 3 },

    // =========================
    // KwaZulu-Natal - Province 4
    // =========================
    new School { SchoolID = 16, SchoolName = "A.M. Moolla Secondary School", ProvinceID = 4 },
    new School { SchoolID = 17, SchoolName = "Abaqulusi High School", ProvinceID = 4 },
    new School { SchoolID = 18, SchoolName = "Amazondi Secondary School", ProvinceID = 4 },
    new School { SchoolID = 19, SchoolName = "Bonga Secondary School", ProvinceID = 4 },
    new School { SchoolID = 20, SchoolName = "Bukelakithi High School", ProvinceID = 4 },

    // =========================
    // Limpopo - Province 5
    // =========================
    new School { SchoolID = 21, SchoolName = "Abel Secondary School", ProvinceID = 5 },
    new School { SchoolID = 22, SchoolName = "Abraham Serote Secondary School", ProvinceID = 5 },
    new School { SchoolID = 23, SchoolName = "Adolf Mhinga Secondary School", ProvinceID = 5 },
    new School { SchoolID = 24, SchoolName = "Alfred Ngwedzeni Secondary School", ProvinceID = 5 },
    new School { SchoolID = 25, SchoolName = "Bambeni Secondary School", ProvinceID = 5 },

    // =========================
    // Mpumalanga - Province 6
    // =========================
    new School { SchoolID = 26, SchoolName = "Acorn-Oaks Comprehensive High School", ProvinceID = 6 },
    new School { SchoolID = 27, SchoolName = "Alfred Matshine Commercial School", ProvinceID = 6 },
    new School { SchoolID = 28, SchoolName = "Amadlelo Aluhlaza Secondary School", ProvinceID = 6 },
    new School { SchoolID = 29, SchoolName = "Bee Maseko Secondary School", ProvinceID = 6 },
    new School { SchoolID = 30, SchoolName = "Ben Matloshe High School", ProvinceID = 6 },

    // =========================
    // Northern Cape - Province 7
    // =========================
    new School { SchoolID = 31, SchoolName = "!Xunkhwesa Combined School", ProvinceID = 7 },
    new School { SchoolID = 32, SchoolName = "Ba Ga Lotlhare Intermediate School", ProvinceID = 7 },
    new School { SchoolID = 33, SchoolName = "Bankhara Bodulong High School", ProvinceID = 7 },
    new School { SchoolID = 34, SchoolName = "Banksdrif Secondary School", ProvinceID = 7 },
    new School { SchoolID = 35, SchoolName = "Kimberley Boys' High School", ProvinceID = 7 },

    // =========================
    // North West - Province 8
    // =========================
    new School { SchoolID = 36, SchoolName = "Areganeng Secondary School", ProvinceID = 8 },
    new School { SchoolID = 37, SchoolName = "Badumedi Secondary School", ProvinceID = 8 },
    new School { SchoolID = 38, SchoolName = "Nqunde Secondary School", ProvinceID = 8 },
    new School { SchoolID = 39, SchoolName = "Ntshidi Secondary School", ProvinceID = 8 },
    new School { SchoolID = 40, SchoolName = "Potchefstroom High School for Boys", ProvinceID = 8 },

    // =========================
    // Western Cape - Province 9
    // =========================
    new School { SchoolID = 41, SchoolName = "Dysselsdorp Sekondêr", ProvinceID = 9 },
    new School { SchoolID = 42, SchoolName = "Fezekile Secondary School", ProvinceID = 9 },
    new School { SchoolID = 43, SchoolName = "Fisantekraal High School", ProvinceID = 9 },
    new School { SchoolID = 44, SchoolName = "Garden Route High School", ProvinceID = 9 },
    new School { SchoolID = 45, SchoolName = "Woodlands Secondary School", ProvinceID = 9 }
);

        }
    }
}
