using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuizFolio.Migrations
{
    /// <inheritdoc />
    public partial class topicAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TopicId",
                table: "Templates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TopicName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Topics",
                columns: new[] { "Id", "TopicName" },
                values: new object[,]
                {
                    { 1, "Education" },
                    { 2, "Quiz" },
                    { 3, "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TopicId",
                table: "Templates",
                column: "TopicId");

            migrationBuilder.AddForeignKey(
                name: "FK_Templates_Topics_TopicId",
                table: "Templates",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Templates_Topics_TopicId",
                table: "Templates");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Templates_TopicId",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "Templates");
        }
    }
}
