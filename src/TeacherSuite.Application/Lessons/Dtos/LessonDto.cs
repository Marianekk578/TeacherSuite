using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Lessons.Dtos;

public class LessonDto
{
    public int Id { get; init; }
    public int CourseId { get; init; }
    public int Order { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DurationMinutes { get; init; }
    public string? AlbumId { get; init; }
    public List<RequirementIconDto> RequirementIcons { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Lesson, LessonDto>()
                .ForMember(dest => dest.RequirementIcons,
                    opt => opt.MapFrom(src => src.LessonRequirementIcons
                        .Select(lr => lr.RequirementIcon)
                        .Where(r => r != null)));
        }
    }
}

public class RequirementIconDto
{
    public string Key { get; init; } = string.Empty;
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
