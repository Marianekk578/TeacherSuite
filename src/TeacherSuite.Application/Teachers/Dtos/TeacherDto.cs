using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Teachers.Dtos;

public class TeacherProgrammingLanguageDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
}

public class TeacherDto
{
    public Guid Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public DateTimeOffset DateOfBirth { get; init; }
    public List<TeacherProgrammingLanguageDto> ProgrammingLanguages { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Teacher, TeacherDto>()
                .ForMember(dest => dest.ProgrammingLanguages,
                    opt => opt.MapFrom(src => src.TeacherProgrammingLanguages
                        .Select(tpl => tpl.ProgrammingLanguage)));

            CreateMap<ProgrammingLanguage, TeacherProgrammingLanguageDto>();
        }
    }
}
