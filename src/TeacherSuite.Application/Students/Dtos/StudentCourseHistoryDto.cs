using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Students.Dtos;

public class StudentCourseHistoryDto
{
    public int CourseId { get; init; }
    public string? CourseName { get; init; }
    public string? GroupName { get; init; }
    public CourseAssignmentStatus Status { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
}
