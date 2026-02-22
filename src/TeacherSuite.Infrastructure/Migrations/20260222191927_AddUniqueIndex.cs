using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeacherSuite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupCourses_GroupId",
                table: "GroupCourses");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCourses_GroupId_CourseId",
                table: "GroupCourses",
                columns: new[] { "GroupId", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GroupCourses_GroupId_CourseId",
                table: "GroupCourses");

            migrationBuilder.CreateIndex(
                name: "IX_GroupCourses_GroupId",
                table: "GroupCourses",
                column: "GroupId");
        }
    }
}
