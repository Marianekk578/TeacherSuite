using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherSuite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAgeGrop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAge",
                table: "AgeGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinAge",
                table: "AgeGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AgeGroups",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAge",
                table: "AgeGroups");

            migrationBuilder.DropColumn(
                name: "MinAge",
                table: "AgeGroups");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AgeGroups");
        }
    }
}
