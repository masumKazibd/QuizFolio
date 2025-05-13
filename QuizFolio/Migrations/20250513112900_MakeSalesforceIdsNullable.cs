using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizFolio.Migrations
{
    /// <inheritdoc />
    public partial class MakeSalesforceIdsNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SalesforceAccountId",
                table: "AspNetUsers",
                nullable: true, // Allow NULL
                oldNullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "SalesforceContactId",
                table: "AspNetUsers",
                nullable: true, // Allow NULL
                oldNullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SalesforceAccountId",
                table: "AspNetUsers",
                nullable: false,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SalesforceContactId",
                table: "AspNetUsers",
                nullable: false,
                oldNullable: true);
        }
    }
}