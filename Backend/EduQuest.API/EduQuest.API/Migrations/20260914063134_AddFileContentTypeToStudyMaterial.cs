using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFileContentTypeToStudyMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileContentType",
                table: "StudyMaterials",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContentType",
                table: "StudyMaterials");
        }
    }
}
