using TeacherSuite.Application.AgeGroups.Dtos;

namespace TeacherSuite.Application.Students.Dtos;

public class StudentDetailGroupDto
{
    public Guid GroupId { get; init; }
    public string? GroupName { get; init; }
    public AgeGroupDto? AgeGroup { get; init; }
}
