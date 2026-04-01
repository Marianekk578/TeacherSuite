using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class LessonSuggestion : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? SelectedText { get; set; }
    public int? SelectionStart { get; set; }
    public int? SelectionEnd { get; set; }
    public ICollection<SuggestionVote> Votes { get; set; } = new List<SuggestionVote>();
}
