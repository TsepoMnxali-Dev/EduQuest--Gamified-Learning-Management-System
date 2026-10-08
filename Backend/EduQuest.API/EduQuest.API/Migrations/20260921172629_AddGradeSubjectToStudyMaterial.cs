using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGradeSubjectToStudyMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TopicID is now optional.
            migrationBuilder.DropForeignKey(
                name: "FK_StudyMaterials_Topics_TopicID",
                table: "StudyMaterials");

            migrationBuilder.AlterColumn<int>(
                name: "TopicID",
                table: "StudyMaterials",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Add GradeSubjectID temporarily as nullable
            // so existing StudyMaterial records can be populated.
            migrationBuilder.AddColumn<int>(
                name: "GradeSubjectID",
                table: "StudyMaterials",
                type: "int",
                nullable: true);

            // Populate GradeSubjectID using the existing Topic relationship.
            //
            // Topic tells us:
            //   - SubjectID
            //   - GradeLevel
            //
            // GradeSubject tells us:
            //   - SubjectID
            //   - GradeID
            //
            // Current data uses:
            //   Topic.GradeLevel = "10", "11", or "12"
            //   Grade.GradeName = "Grade 10", "Grade 11", or "Grade 12"
            migrationBuilder.Sql(
                """
                UPDATE sm
                SET sm.GradeSubjectID = gs.GradeSubjectID
                FROM StudyMaterials sm
                INNER JOIN Topics t
                    ON sm.TopicID = t.TopicID
                INNER JOIN Grades g
                    ON g.GradeName = t.GradeLevel
                INNER JOIN GradeSubjects gs
                    ON gs.SubjectID = t.SubjectID
                    AND gs.GradeID = g.GradeID
                WHERE sm.GradeSubjectID IS NULL;
                """);

            // Every existing StudyMaterial should now have a GradeSubjectID.
            // The following check prevents the migration from silently
            // creating invalid records.
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM StudyMaterials
                    WHERE GradeSubjectID IS NULL
                )
                BEGIN
                    THROW 50001,
                        'Migration failed: one or more StudyMaterials could not be assigned to a GradeSubject.',
                        1;
                END;
                """);

            // Now that all existing records have a value,
            // make GradeSubjectID required.
            migrationBuilder.AlterColumn<int>(
                name: "GradeSubjectID",
                table: "StudyMaterials",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterials_GradeSubjectID",
                table: "StudyMaterials",
                column: "GradeSubjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyMaterials_GradeSubjects_GradeSubjectID",
                table: "StudyMaterials",
                column: "GradeSubjectID",
                principalTable: "GradeSubjects",
                principalColumn: "GradeSubjectID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyMaterials_Topics_TopicID",
                table: "StudyMaterials",
                column: "TopicID",
                principalTable: "Topics",
                principalColumn: "TopicID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudyMaterials_GradeSubjects_GradeSubjectID",
                table: "StudyMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyMaterials_Topics_TopicID",
                table: "StudyMaterials");

            migrationBuilder.DropIndex(
                name: "IX_StudyMaterials_GradeSubjectID",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "GradeSubjectID",
                table: "StudyMaterials");

            migrationBuilder.AlterColumn<int>(
                name: "TopicID",
                table: "StudyMaterials",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyMaterials_Topics_TopicID",
                table: "StudyMaterials",
                column: "TopicID",
                principalTable: "Topics",
                principalColumn: "TopicID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}