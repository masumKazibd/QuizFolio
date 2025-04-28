using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizFolio.Migrations
{
    /// <inheritdoc />
    public partial class CreateFormResponseTable11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswerOptionsJson",
                table: "FormResponses");

            migrationBuilder.DropColumn(
                name: "AnswerText",
                table: "FormResponses");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "FormResponses");

            migrationBuilder.AddColumn<string>(
                name: "AnswersJson",
                table: "FormResponses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnswersJson",
                table: "FormResponses");

            migrationBuilder.AddColumn<string>(
                name: "AnswerOptionsJson",
                table: "FormResponses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnswerText",
                table: "FormResponses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionId",
                table: "FormResponses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
