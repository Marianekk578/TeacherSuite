namespace TeacherSuite.Domain.Entities;

public class AgeGroup
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Label { get; set; }
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
}