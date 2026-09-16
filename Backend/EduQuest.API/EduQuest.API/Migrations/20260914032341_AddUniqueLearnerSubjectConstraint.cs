using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueLearnerSubjectConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_learnerSubjects_LearnerID",
                table: "learnerSubjects");

            migrationBuilder.DropColumn(
                name: "GradeLevel",
                table: "learnerSubjects");

            migrationBuilder.CreateIndex(
                name: "IX_learnerSubjects_LearnerID_SubjectID",
                table: "learnerSubjects",
                columns: new[] { "LearnerID", "SubjectID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_learnerSubjects_LearnerID_SubjectID",
                table: "learnerSubjects");

            migrationBuilder.AddColumn<string>(
                name: "GradeLevel",
                table: "learnerSubjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_learnerSubjects_LearnerID",
                table: "learnerSubjects",
                column: "LearnerID");
        }
    }
}
