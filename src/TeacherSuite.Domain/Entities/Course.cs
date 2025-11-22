namespace TeacherSuite.Domain.Entities;

public class Course
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int AgeGroupID { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public ICollection<CourseProgrammingLanguage> CourseProgrammingLanguages { get; set; } = new List<CourseProgrammingLanguage>();
    public ICollection<GroupCourse> GroupCourses { get; set; } = new List<GroupCourse>();
}