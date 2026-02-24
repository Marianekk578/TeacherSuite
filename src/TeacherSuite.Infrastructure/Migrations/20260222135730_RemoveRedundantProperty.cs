using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherSuite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "GroupCourses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "GroupCourses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
