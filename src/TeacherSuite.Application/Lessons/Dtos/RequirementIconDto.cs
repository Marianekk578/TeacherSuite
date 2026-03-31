using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Lessons.Dtos;

public class RequirementIconDto
{
    public int Id { get; init; }
    public string Emoji { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<RequirementIcon, RequirementIconDto>();
        }
    }
}
