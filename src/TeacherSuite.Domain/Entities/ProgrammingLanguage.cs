namespace TeacherSuite.Domain.Entities;

public class ProgrammingLanguage
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ICollection<TeacherProgrammingLanguage> TeacherProgrammingLanguages { get; set; } = new List<TeacherProgrammingLanguage>();
    public ICollection<CourseProgrammingLanguage> CourseProgrammingLanguages { get; set; } = new List<CourseProgrammingLanguage>();
}