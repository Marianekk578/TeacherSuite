using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Domain.Entities;

public class SuggestionVote
{
    public Guid SuggestionId { get; set; }
    public LessonSuggestion? Suggestion { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public VoteType Vote { get; set; }
}
