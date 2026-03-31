using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Lessons.Dtos;

public class LessonDetailDto
{
    public int Id { get; init; }
    public int CourseId { get; init; }
    public int Order { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DurationMinutes { get; init; }
    public string? AlbumId { get; init; }
    public List<RequirementIconDto> RequirementIcons { get; init; } = new();
    public string? CourseName { get; init; }
    public List<LessonSuggestionDto> Suggestions { get; init; } = new();
    public List<LessonAttendanceDto> Attendances { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Lesson, LessonDetailDto>()
                .ForMember(dest => dest.RequirementIcons,
                    opt => opt.MapFrom(src => src.LessonRequirementIcons
                        .Select(lr => lr.RequirementIcon)
                        .Where(r => r != null)))
                .ForMember(dest => dest.CourseName,
                    opt => opt.MapFrom(src => src.Course != null ? src.Course.Name : null));
        }
    }
}
