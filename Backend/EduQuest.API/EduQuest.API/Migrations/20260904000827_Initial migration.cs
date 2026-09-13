using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class Initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    AchievementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PointsRequired = table.Column<int>(type: "int", nullable: false),
                    BadgeImage = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.AchievementID);
                });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    GradeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.GradeID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    SponsorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.SponsorID);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    SubjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.SubjectID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Competitions",
                columns: table => new
                {
                    CompetitionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SponsorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SponsorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competitions", x => x.CompetitionID);
                    table.ForeignKey(
                        name: "FK_Competitions_Sponsors_SponsorID",
                        column: x => x.SponsorID,
                        principalTable: "Sponsors",
                        principalColumn: "SponsorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GradeSubjects",
                columns: table => new
                {
                    GradeSubjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeSubjects", x => x.GradeSubjectID);
                    table.ForeignKey(
                        name: "FK_GradeSubjects_Grades_GradeID",
                        column: x => x.GradeID,
                        principalTable: "Grades",
                        principalColumn: "GradeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GradeSubjects_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudyMaterials",
                columns: table => new
                {
                    StudyMaterialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    GradeID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyMaterials", x => x.StudyMaterialID);
                    table.ForeignKey(
                        name: "FK_StudyMaterials_Grades_GradeID",
                        column: x => x.GradeID,
                        principalTable: "Grades",
                        principalColumn: "GradeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudyMaterials_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    TopicID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    TopicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.TopicID);
                    table.ForeignKey(
                        name: "FK_Topics_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activityLogs",
                columns: table => new
                {
                    ActivityLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activityLogs", x => x.ActivityLogID);
                    table.ForeignKey(
                        name: "FK_activityLogs_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Learners",
                columns: table => new
                {
                    LearnerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    GradeID = table.Column<int>(type: "int", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Province = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Learners", x => x.LearnerID);
                    table.ForeignKey(
                        name: "FK_Learners_Grades_GradeID",
                        column: x => x.GradeID,
                        principalTable: "Grades",
                        principalColumn: "GradeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Learners_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prizes",
                columns: table => new
                {
                    PrizeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrizeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrizeDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    CompetitionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prizes", x => x.PrizeID);
                    table.ForeignKey(
                        name: "FK_Prizes_Competitions_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competitions",
                        principalColumn: "CompetitionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    QuizID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeLimit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    TopicID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.QuizID);
                    table.ForeignKey(
                        name: "FK_Quizzes_Topics_TopicID",
                        column: x => x.TopicID,
                        principalTable: "Topics",
                        principalColumn: "TopicID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompetitionLearners",
                columns: table => new
                {
                    CompetitionLearnerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompetitionID = table.Column<int>(type: "int", nullable: true),
                    LearnerID = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitionLearners", x => x.CompetitionLearnerID);
                    table.ForeignKey(
                        name: "FK_CompetitionLearners_Competitions_CompetitionID",
                        column: x => x.CompetitionID,
                        principalTable: "Competitions",
                        principalColumn: "CompetitionID");
                    table.ForeignKey(
                        name: "FK_CompetitionLearners_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "LearnerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderBoards",
                columns: table => new
                {
                    LeaderboardID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearnerID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderBoards", x => x.LeaderboardID);
                    table.ForeignKey(
                        name: "FK_LeaderBoards_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "LearnerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LearnerAchievements",
                columns: table => new
                {
                    LearnerAchievementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerID = table.Column<int>(type: "int", nullable: false),
                    AchievementID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearnerAchievements", x => x.LearnerAchievementID);
                    table.ForeignKey(
                        name: "FK_LearnerAchievements_Achievements_AchievementID",
                        column: x => x.AchievementID,
                        principalTable: "Achievements",
                        principalColumn: "AchievementID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearnerAchievements_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "LearnerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "learnerSubjects",
                columns: table => new
                {
                    LearnerSubjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LearnerID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learnerSubjects", x => x.LearnerSubjectID);
                    table.ForeignKey(
                        name: "FK_learnerSubjects_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "LearnerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_learnerSubjects_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "SubjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    DateSent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeanerID = table.Column<int>(type: "int", nullable: false),
                    LearnerID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "LearnerID");
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    QuizAttemptID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Score = table.Column<int>(type: "int", nullable: false),
                    DateTaken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeTaken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuizID = table.Column<int>(type: "int", nullable: false),
                    LearnerID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.QuizAttemptID);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Learners_LearnerID",
                        column: x => x.LearnerID,
                        principalTable: "Learners",
                        principalColumn: "LearnerID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    QuizQuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedByAI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApprovedByAdmin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuizID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.QuizQuestionID);
                    table.ForeignKey(
                        name: "FK_QuizQuestions_Quizzes_QuizID",
                        column: x => x.QuizID,
                        principalTable: "Quizzes",
                        principalColumn: "QuizID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizOptions",
                columns: table => new
                {
                    QuizOptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    QuizQuestionID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizOptions", x => x.QuizOptionID);
                    table.ForeignKey(
                        name: "FK_QuizOptions_QuizQuestions_QuizQuestionID",
                        column: x => x.QuizQuestionID,
                        principalTable: "QuizQuestions",
                        principalColumn: "QuizQuestionID");
                });

            migrationBuilder.CreateTable(
                name: "QuizAttemptAnswers",
                columns: table => new
                {
                    QuizAttemptAnswerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizOptionID = table.Column<int>(type: "int", nullable: false),
                    QuestionID = table.Column<int>(type: "int", nullable: false),
                    QuizQuestionID = table.Column<int>(type: "int", nullable: true),
                    AttemptID = table.Column<int>(type: "int", nullable: false),
                    QuizAttemptID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptAnswers", x => x.QuizAttemptAnswerID);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizAttempts_QuizAttemptID",
                        column: x => x.QuizAttemptID,
                        principalTable: "QuizAttempts",
                        principalColumn: "QuizAttemptID");
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizOptions_QuizOptionID",
                        column: x => x.QuizOptionID,
                        principalTable: "QuizOptions",
                        principalColumn: "QuizOptionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttemptAnswers_QuizQuestions_QuizQuestionID",
                        column: x => x.QuizQuestionID,
                        principalTable: "QuizQuestions",
                        principalColumn: "QuizQuestionID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_activityLogs_UserID",
                table: "activityLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionLearners_CompetitionID",
                table: "CompetitionLearners",
                column: "CompetitionID");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitionLearners_LearnerID",
                table: "CompetitionLearners",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Competitions_SponsorID",
                table: "Competitions",
                column: "SponsorID");

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjects_GradeID",
                table: "GradeSubjects",
                column: "GradeID");

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjects_SubjectID",
                table: "GradeSubjects",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderBoards_LearnerID",
                table: "LeaderBoards",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAchievements_AchievementID",
                table: "LearnerAchievements",
                column: "AchievementID");

            migrationBuilder.CreateIndex(
                name: "IX_LearnerAchievements_LearnerID",
                table: "LearnerAchievements",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_GradeID",
                table: "Learners",
                column: "GradeID");

            migrationBuilder.CreateIndex(
                name: "IX_Learners_UserID",
                table: "Learners",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_learnerSubjects_LearnerID",
                table: "learnerSubjects",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_learnerSubjects_SubjectID",
                table: "learnerSubjects",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_LearnerID",
                table: "Notifications",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Prizes_CompetitionID",
                table: "Prizes",
                column: "CompetitionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_QuizAttemptID",
                table: "QuizAttemptAnswers",
                column: "QuizAttemptID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_QuizOptionID",
                table: "QuizAttemptAnswers",
                column: "QuizOptionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptAnswers_QuizQuestionID",
                table: "QuizAttemptAnswers",
                column: "QuizQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_LearnerID",
                table: "QuizAttempts",
                column: "LearnerID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizID",
                table: "QuizAttempts",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizOptions_QuizQuestionID",
                table: "QuizOptions",
                column: "QuizQuestionID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizQuestions_QuizID",
                table: "QuizQuestions",
                column: "QuizID");

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_TopicID",
                table: "Quizzes",
                column: "TopicID");

            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterials_GradeID",
                table: "StudyMaterials",
                column: "GradeID");

            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterials_SubjectID",
                table: "StudyMaterials",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_SubjectID",
                table: "Topics",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activityLogs");

            migrationBuilder.DropTable(
                name: "CompetitionLearners");

            migrationBuilder.DropTable(
                name: "GradeSubjects");

            migrationBuilder.DropTable(
                name: "LeaderBoards");

            migrationBuilder.DropTable(
                name: "LearnerAchievements");

            migrationBuilder.DropTable(
                name: "learnerSubjects");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Prizes");

            migrationBuilder.DropTable(
                name: "QuizAttemptAnswers");

            migrationBuilder.DropTable(
                name: "StudyMaterials");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "Competitions");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizOptions");

            migrationBuilder.DropTable(
                name: "Sponsors");

            migrationBuilder.DropTable(
                name: "Learners");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
