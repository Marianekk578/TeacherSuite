namespace TeacherSuite.Application.Students.Dtos;

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
