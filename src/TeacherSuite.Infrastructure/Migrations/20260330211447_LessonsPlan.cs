using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeacherSuite.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LessonsPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkdownContent",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "MaterialFileName",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "MaterialStorageKey",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "MaterialType",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "RequirementIcons",
                table: "Lessons");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Created",
                table: "Lessons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Lessons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModified",
                table: "Lessons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Lessons",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RequirementIcons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Emoji = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequirementIcons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledLessons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledStart = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ScheduledEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledLessons_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduledLessons_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonRequirementIcons",
                columns: table => new
                {
                    LessonId = table.Column<int>(type: "integer", nullable: false),
                    RequirementIconId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonRequirementIcons", x => new { x.LessonId, x.RequirementIconId });
                    table.ForeignKey(
                        name: "FK_LessonRequirementIcons_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonRequirementIcons_RequirementIcons_RequirementIconId",
                        column: x => x.RequirementIconId,
                        principalTable: "RequirementIcons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentLessonAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledLessonId = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPresent = table.Column<bool>(type: "boolean", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLessonAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLessonAttendances_ScheduledLessons_ScheduledLessonId",
                        column: x => x.ScheduledLessonId,
                        principalTable: "ScheduledLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentLessonAttendances_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonRequirementIcons_RequirementIconId",
                table: "LessonRequirementIcons",
                column: "RequirementIconId");

            migrationBuilder.CreateIndex(
                name: "IX_RequirementIcons_Key",
                table: "RequirementIcons",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_GroupId",
                table: "ScheduledLessons",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledLessons_LessonId_GroupId_ScheduledStart",
                table: "ScheduledLessons",
                columns: new[] { "LessonId", "GroupId", "ScheduledStart" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentLessonAttendances_ScheduledLessonId_StudentId",
                table: "StudentLessonAttendances",
                columns: new[] { "ScheduledLessonId", "StudentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentLessonAttendances_StudentId",
                table: "StudentLessonAttendances",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonRequirementIcons");

            migrationBuilder.DropTable(
                name: "StudentLessonAttendances");

            migrationBuilder.DropTable(
                name: "RequirementIcons");

            migrationBuilder.DropTable(
                name: "ScheduledLessons");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Lessons");

            migrationBuilder.AddColumn<string>(
                name: "MarkdownContent",
                table: "Lessons",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialFileName",
                table: "Lessons",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialStorageKey",
                table: "Lessons",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialType",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequirementIcons",
                table: "Lessons",
                type: "text",
                nullable: true);
        }
    }
}
