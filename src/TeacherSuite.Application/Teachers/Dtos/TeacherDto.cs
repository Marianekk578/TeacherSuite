using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Teachers.Dtos;

public class TeacherDto
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Teacher, TeacherDto>();
        }
    }
}
