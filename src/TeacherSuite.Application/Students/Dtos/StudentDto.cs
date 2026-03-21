using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Students.Dtos;

public class StudentDto
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTimeOffset DateOfBirth { get; init; }
    public string ContactEmail { get; init; } = string.Empty;
    public string ContactPhone { get; init; } = string.Empty;
    public string? ParentFirstName { get; init; }
    public string? ParentLastName { get; init; }
    public List<StudentGroupDto> Groups { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Student, StudentDto>()
                .ForMember(dest => dest.Groups, opt => opt.MapFrom(src => src.StudentGroups));
        }
    }
}
