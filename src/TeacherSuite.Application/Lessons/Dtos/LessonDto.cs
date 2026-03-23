using System.Text.Json;
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
    public int MaterialType { get; init; }
    public List<string> RequirementIcons { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Lesson, LessonDto>()
                .ForMember(dest => dest.MaterialType,
                    opt => opt.MapFrom(src => (int)src.MaterialType))
                .ForMember(dest => dest.RequirementIcons,
                    opt => opt.MapFrom(src => string.IsNullOrEmpty(src.RequirementIcons)
                        ? new List<string>()
                        : JsonSerializer.Deserialize<List<string>>(src.RequirementIcons, (JsonSerializerOptions?)null) ?? new List<string>()));
        }
    }
}
