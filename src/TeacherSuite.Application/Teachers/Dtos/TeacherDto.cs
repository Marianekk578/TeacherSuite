using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Teachers.Dtos;

public class TeacherDto
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTimeOffset DateOfBirth { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Teacher, TeacherDto>();
        }
    }
}
