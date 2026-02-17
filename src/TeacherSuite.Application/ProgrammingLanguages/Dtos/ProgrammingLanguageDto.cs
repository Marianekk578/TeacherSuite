using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.ProgrammingLanguages.Dtos;

public class ProgrammingLanguageDto
{
    public int Id { get; init; }
    public string? Name { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ProgrammingLanguage, ProgrammingLanguageDto>();
        }
    }
}
