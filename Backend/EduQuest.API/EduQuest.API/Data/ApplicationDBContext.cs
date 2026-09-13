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

    }
}
