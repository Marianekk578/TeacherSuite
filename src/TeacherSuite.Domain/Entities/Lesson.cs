using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Domain.Entities;

public class Lesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; } = 90;
    public LessonMaterialType MaterialType { get; set; } = LessonMaterialType.None;
    public string? MarkdownContent { get; set; }
    public string? MaterialFileName { get; set; }
    public string? MaterialStorageKey { get; set; }
    public string? AlbumId { get; set; }
    public string? RequirementIcons { get; set; }
    public ICollection<LessonAttendance> Attendances { get; set; } = new List<LessonAttendance>();
    public ICollection<LessonSuggestion> Suggestions { get; set; } = new List<LessonSuggestion>();
}
