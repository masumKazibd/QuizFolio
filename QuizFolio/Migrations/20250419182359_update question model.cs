using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizFolio.Migrations
{
    /// <inheritdoc />
    public partial class updatequestionmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Questions",
                newName: "QuestionType");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "Questions",
                newName: "QuestionTitle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuestionType",
                table: "Questions",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "QuestionTitle",
                table: "Questions",
                newName: "Text");
        }
    }
}
