using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Teachers.Dtos;

public class TeacherProgrammingLanguageDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Label { get; init; }
    public string? Color { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ProgrammingLanguage, TeacherProgrammingLanguageDto>();
        }
    }
}
