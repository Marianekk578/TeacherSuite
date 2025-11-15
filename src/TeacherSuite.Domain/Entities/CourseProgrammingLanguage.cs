namespace TeacherSuite.Domain.Entities;

public class CourseProgrammingLanguage
{
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int ProgrammingLanguageId { get; set; }
    public ProgrammingLanguage? ProgrammingLanguage { get; set; }
}