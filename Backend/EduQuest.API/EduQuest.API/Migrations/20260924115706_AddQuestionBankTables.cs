using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuest.API.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionBankTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionBankItems",
                columns: table => new
                {
                    QuestionBankItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TopicID = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceExtract = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Difficulty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankItems", x => x.QuestionBankItemID);
                    table.ForeignKey(
                        name: "FK_QuestionBankItems_Topics_TopicID",
                        column: x => x.TopicID,
                        principalTable: "Topics",
                        principalColumn: "TopicID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionBankOptions",
                columns: table => new
                {
                    QuestionBankOptionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OptionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    QuestionBankItemID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionBankOptions", x => x.QuestionBankOptionID);
                    table.ForeignKey(
                        name: "FK_QuestionBankOptions_QuestionBankItems_QuestionBankItemID",
                        column: x => x.QuestionBankItemID,
                        principalTable: "QuestionBankItems",
                        principalColumn: "QuestionBankItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_TopicID",
                table: "QuestionBankItems",
                column: "TopicID");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankOptions_QuestionBankItemID",
                table: "QuestionBankOptions",
                column: "QuestionBankItemID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionBankOptions");

            migrationBuilder.DropTable(
                name: "QuestionBankItems");
        }
    }
}
