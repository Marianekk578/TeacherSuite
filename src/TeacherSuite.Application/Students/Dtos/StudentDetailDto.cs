using TeacherSuite.Application.AgeGroups.Dtos;
using TeacherSuite.Domain.Entities;
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

public class StudentProgrammingLanguageDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Label { get; init; }
    public string? Color { get; init; }
}

public class StudentDetailGroupDto
{
    public Guid GroupId { get; init; }
    public string? GroupName { get; init; }
    public AgeGroupDto? AgeGroup { get; init; }
}

public class StudentDetailDto
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTimeOffset DateOfBirth { get; init; }
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public string? ParentFirstName { get; init; }
    public string? ParentLastName { get; init; }
    public List<StudentDetailGroupDto> Groups { get; init; } = new();
    public List<StudentCourseHistoryDto> CourseHistory { get; init; } = new();
    public List<StudentProgrammingLanguageDto> ProgrammingLanguages { get; init; } = new();
}
