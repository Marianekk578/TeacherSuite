using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class StudentGroup : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid GroupId { get; set; }
    public Group? Group { get; set; }
}
