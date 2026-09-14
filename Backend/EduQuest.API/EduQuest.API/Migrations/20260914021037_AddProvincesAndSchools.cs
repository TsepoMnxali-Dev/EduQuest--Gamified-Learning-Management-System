using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProvincesAndSchools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Province",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                table: "Learners");

            migrationBuilder.AddColumn<int>(
                name: "SchoolID",
                table: "Learners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    ProvinceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProvinceName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.ProvinceID);
                });

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    SchoolID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProvinceID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.SchoolID);
                    table.ForeignKey(
                        name: "FK_Schools_Provinces_ProvinceID",
                        column: x => x.ProvinceID,
                        principalTable: "Provinces",
                        principalColumn: "ProvinceID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Provinces",
                columns: new[] { "ProvinceID", "ProvinceName" },
                values: new object[,]
                {
                    { 1, "Eastern Cape" },
                    { 2, "Free State" },
                    { 3, "Gauteng" },
                    { 4, "KwaZulu-Natal" },
                    { 5, "Limpopo" },
                    { 6, "Mpumalanga" },
                    { 7, "Northern Cape" },
                    { 8, "North West" },
                    { 9, "Western Cape" }
                });

            migrationBuilder.InsertData(
                table: "Schools",
                columns: new[] { "SchoolID", "ProvinceID", "SchoolName" },
                values: new object[,]
                {
                    { 1, 1, "EduQuest Sample School - Eastern Cape" },
                    { 2, 3, "EduQuest Sample School - Gauteng" },
                    { 3, 9, "EduQuest Sample School - Western Cape" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Learners_SchoolID",
                table: "Learners",
                column: "SchoolID");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_ProvinceID",
                table: "Schools",
                column: "ProvinceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_Schools_SchoolID",
                table: "Learners",
                column: "SchoolID",
                principalTable: "Schools",
                principalColumn: "SchoolID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_Schools_SchoolID",
                table: "Learners");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropIndex(
                name: "IX_Learners_SchoolID",
                table: "Learners");

            migrationBuilder.DropColumn(
                name: "SchoolID",
                table: "Learners");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Learners",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                table: "Learners",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
