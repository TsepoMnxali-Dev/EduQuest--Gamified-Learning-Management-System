using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeQuizQuestionFlagsToBoolean : Migration
    {
        /// <inheritdoc />

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GeneratedByAI_New",
                table: "QuizQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ApprovedByAdmin_New",
                table: "QuizQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
        UPDATE QuizQuestions
        SET GeneratedByAI_New =
                CASE
                    WHEN GeneratedByAI = 'Yes' THEN 1
                    ELSE 0
                END,
            ApprovedByAdmin_New =
                CASE
                    WHEN ApprovedByAdmin = 'Yes' THEN 1
                    ELSE 0
                END;
        """);

            migrationBuilder.DropColumn(
                name: "GeneratedByAI",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "ApprovedByAdmin",
                table: "QuizQuestions");

            migrationBuilder.RenameColumn(
                name: "GeneratedByAI_New",
                table: "QuizQuestions",
                newName: "GeneratedByAI");

            migrationBuilder.RenameColumn(
                name: "ApprovedByAdmin_New",
                table: "QuizQuestions",
                newName: "ApprovedByAdmin");

            migrationBuilder.CreateTable(
                name: "QuizAttemptQuestions",
                columns: table => new
                {
                    QuizAttemptQuestionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizAttemptID = table.Column<int>(type: "int", nullable: false),
                    QuizQuestionID = table.Column<int>(type: "int", nullable: false),
                    QuestionOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_QuizAttemptQuestions",
                        x => x.QuizAttemptQuestionID);

                    table.ForeignKey(
                        name: "FK_QuizAttemptQuestions_QuizAttempts_QuizAttemptID",
                        column: x => x.QuizAttemptID,
                        principalTable: "QuizAttempts",
                        principalColumn: "QuizAttemptID",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_QuizAttemptQuestions_QuizQuestions_QuizQuestionID",
                        column: x => x.QuizQuestionID,
                        principalTable: "QuizQuestions",
                        principalColumn: "QuizQuestionID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptQuestions_QuizAttemptID",
                table: "QuizAttemptQuestions",
                column: "QuizAttemptID");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptQuestions_QuizQuestionID",
                table: "QuizAttemptQuestions",
                column: "QuizQuestionID");
        }
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneratedByAI_Old",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "No");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByAdmin_Old",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "No");

            migrationBuilder.Sql("""
        UPDATE QuizQuestions
        SET GeneratedByAI_Old =
                CASE
                    WHEN GeneratedByAI = 1 THEN 'Yes'
                    ELSE 'No'
                END,
            ApprovedByAdmin_Old =
                CASE
                    WHEN ApprovedByAdmin = 1 THEN 'Yes'
                    ELSE 'No'
                END;
        """);

            migrationBuilder.DropColumn(
                name: "GeneratedByAI",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "ApprovedByAdmin",
                table: "QuizQuestions");

            migrationBuilder.RenameColumn(
                name: "GeneratedByAI_Old",
                table: "QuizQuestions",
                newName: "GeneratedByAI");

            migrationBuilder.RenameColumn(
                name: "ApprovedByAdmin_Old",
                table: "QuizQuestions",
                newName: "ApprovedByAdmin");

            migrationBuilder.DropTable(
                name: "QuizAttemptQuestions");
        }
    }
}
