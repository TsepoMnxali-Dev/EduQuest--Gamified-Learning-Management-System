using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudyMaterialStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudyMaterials_Grades_GradeID",
                table: "StudyMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_StudyMaterials_Subjects_SubjectID",
                table: "StudyMaterials");

            migrationBuilder.DropIndex(
                name: "IX_StudyMaterials_GradeID",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "GradeID",
                table: "StudyMaterials");

            migrationBuilder.RenameColumn(
                name: "SubjectID",
                table: "StudyMaterials",
                newName: "TopicID");

            migrationBuilder.RenameIndex(
                name: "IX_StudyMaterials_SubjectID",
                table: "StudyMaterials",
                newName: "IX_StudyMaterials_TopicID");

            migrationBuilder.AlterColumn<string>(
                name: "FileURL",
                table: "StudyMaterials",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StudyMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "StudyMaterials",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "StudyMaterials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyMaterials_Topics_TopicID",
                table: "StudyMaterials",
                column: "TopicID",
                principalTable: "Topics",
                principalColumn: "TopicID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudyMaterials_Topics_TopicID",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "FileData",
                table: "StudyMaterials");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "StudyMaterials");

            migrationBuilder.RenameColumn(
                name: "TopicID",
                table: "StudyMaterials",
                newName: "SubjectID");

            migrationBuilder.RenameIndex(
                name: "IX_StudyMaterials_TopicID",
                table: "StudyMaterials",
                newName: "IX_StudyMaterials_SubjectID");

            migrationBuilder.AlterColumn<string>(
                name: "FileURL",
                table: "StudyMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GradeID",
                table: "StudyMaterials",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StudyMaterials_GradeID",
                table: "StudyMaterials",
                column: "GradeID");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyMaterials_Grades_GradeID",
                table: "StudyMaterials",
                column: "GradeID",
                principalTable: "Grades",
                principalColumn: "GradeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudyMaterials_Subjects_SubjectID",
                table: "StudyMaterials",
                column: "SubjectID",
                principalTable: "Subjects",
                principalColumn: "SubjectID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
