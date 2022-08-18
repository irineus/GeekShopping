using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeekShopping.Email.Migrations
{
    public partial class AlterEmailTableOnDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "log",
                table: "email_logs",
                newName: "to");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "email_logs",
                newName: "subject");

            migrationBuilder.AddColumn<string>(
                name: "body",
                table: "email_logs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "from",
                table: "email_logs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "body",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "from",
                table: "email_logs");

            migrationBuilder.RenameColumn(
                name: "to",
                table: "email_logs",
                newName: "log");

            migrationBuilder.RenameColumn(
                name: "subject",
                table: "email_logs",
                newName: "email");
        }
    }
}
