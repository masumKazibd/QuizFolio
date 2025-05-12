using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizFolio.Migrations
{
    /// <inheritdoc />
    public partial class salesforce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReceiveProductUpdates",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SalesforceAccountId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SalesforceContactId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SalesforceLastSync",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SubscribeToNewsletter",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiveProductUpdates",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SalesforceAccountId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SalesforceContactId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SalesforceLastSync",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscribeToNewsletter",
                table: "AspNetUsers");
        }
    }
}
