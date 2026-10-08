using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceTestSchoolsWithRealSchools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_Users_UserID",
                table: "Learners");

            migrationBuilder.DropIndex(
                name: "IX_Learners_UserID",
                table: "Learners");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 1,
                column: "SchoolName",
                value: "AM Zantsi Senior Secondary School");

            migrationBuilder.UpdateData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 2,
                columns: new[] { "ProvinceID", "SchoolName" },
                values: new object[] { 1, "Amabele Senior Secondary School" });

            migrationBuilder.UpdateData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 3,
                columns: new[] { "ProvinceID", "SchoolName" },
                values: new object[] { 1, "Emdemi Senior Secondary School" });

            migrationBuilder.InsertData(
                table: "Schools",
                columns: new[] { "SchoolID", "ProvinceID", "SchoolName" },
                values: new object[,]
                {
                    { 4, 1, "St Margaret Senior Secondary School" },
                    { 5, 1, "St Matthews High School" },
                    { 6, 2, "Sehlabeng Secondary School" },
                    { 7, 2, "Sehunelo Secondary School" },
                    { 8, 2, "Selelekelela Secondary School" },
                    { 9, 2, "Seotlong A Secondary School" },
                    { 10, 2, "Teto Secondary School" },
                    { 11, 3, "Adam Masebe Secondary School" },
                    { 12, 3, "Altmont Technical High School" },
                    { 13, 3, "Asser Maloka Secondary School" },
                    { 14, 3, "Bona Lesedi Secondary School" },
                    { 15, 3, "Cosmo City Secondary School" },
                    { 16, 4, "A.M. Moolla Secondary School" },
                    { 17, 4, "Abaqulusi High School" },
                    { 18, 4, "Amazondi Secondary School" },
                    { 19, 4, "Bonga Secondary School" },
                    { 20, 4, "Bukelakithi High School" },
                    { 21, 5, "Abel Secondary School" },
                    { 22, 5, "Abraham Serote Secondary School" },
                    { 23, 5, "Adolf Mhinga Secondary School" },
                    { 24, 5, "Alfred Ngwedzeni Secondary School" },
                    { 25, 5, "Bambeni Secondary School" },
                    { 26, 6, "Acorn-Oaks Comprehensive High School" },
                    { 27, 6, "Alfred Matshine Commercial School" },
                    { 28, 6, "Amadlelo Aluhlaza Secondary School" },
                    { 29, 6, "Bee Maseko Secondary School" },
                    { 30, 6, "Ben Matloshe High School" },
                    { 31, 7, "!Xunkhwesa Combined School" },
                    { 32, 7, "Ba Ga Lotlhare Intermediate School" },
                    { 33, 7, "Bankhara Bodulong High School" },
                    { 34, 7, "Banksdrif Secondary School" },
                    { 35, 7, "Kimberley Boys' High School" },
                    { 36, 8, "Areganeng Secondary School" },
                    { 37, 8, "Badumedi Secondary School" },
                    { 38, 8, "Nqunde Secondary School" },
                    { 39, 8, "Ntshidi Secondary School" },
                    { 40, 8, "Potchefstroom High School for Boys" },
                    { 41, 9, "Dysselsdorp Sekondêr" },
                    { 42, 9, "Fezekile Secondary School" },
                    { 43, 9, "Fisantekraal High School" },
                    { 44, 9, "Garden Route High School" },
                    { 45, 9, "Woodlands Secondary School" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Learners_UserID",
                table: "Learners",
                column: "UserID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_Users_UserID",
                table: "Learners",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Learners_Users_UserID",
                table: "Learners");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Learners_UserID",
                table: "Learners");

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 45);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.UpdateData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 1,
                column: "SchoolName",
                value: "EduQuest Sample School - Eastern Cape");

            migrationBuilder.UpdateData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 2,
                columns: new[] { "ProvinceID", "SchoolName" },
                values: new object[] { 3, "EduQuest Sample School - Gauteng" });

            migrationBuilder.UpdateData(
                table: "Schools",
                keyColumn: "SchoolID",
                keyValue: 3,
                columns: new[] { "ProvinceID", "SchoolName" },
                values: new object[] { 9, "EduQuest Sample School - Western Cape" });

            migrationBuilder.CreateIndex(
                name: "IX_Learners_UserID",
                table: "Learners",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Learners_Users_UserID",
                table: "Learners",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
