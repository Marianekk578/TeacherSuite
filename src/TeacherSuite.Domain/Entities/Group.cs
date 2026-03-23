using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class Group : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public int AgeGroupID { get; set; }
    public AgeGroup? AgeGroup { get; set; }
    public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
    public ICollection<GroupCourse> GroupCourses { get; set; } = new List<GroupCourse>();
}