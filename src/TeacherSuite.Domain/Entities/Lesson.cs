using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class Lesson : BaseAuditableEntity
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int Order { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; } = 90;
    public string? AlbumId { get; set; }
    public ICollection<LessonRequirementIcon> LessonRequirementIcons { get; set; } = new List<LessonRequirementIcon>();
    public ICollection<LessonAttendance> Attendances { get; set; } = new List<LessonAttendance>();
    public ICollection<LessonSuggestion> Suggestions { get; set; } = new List<LessonSuggestion>();
}
