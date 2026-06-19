namespace TeacherSuite.Domain.Entities;

public class RequirementIcon
{
    public int Id { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public ICollection<LessonRequirementIcon> LessonRequirementIcons { get; set; } = new List<LessonRequirementIcon>();
}
