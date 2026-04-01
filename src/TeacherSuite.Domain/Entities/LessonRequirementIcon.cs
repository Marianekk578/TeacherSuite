namespace TeacherSuite.Domain.Entities;

public class LessonRequirementIcon
{
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int RequirementIconId { get; set; }
    public RequirementIcon? RequirementIcon { get; set; }
}
