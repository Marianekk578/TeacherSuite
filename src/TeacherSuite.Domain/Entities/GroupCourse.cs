using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Domain.Entities;

public class GroupCourse : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Group? Group { get; set; }
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public CourseAssignmentStatus Status { get; set; } = CourseAssignmentStatus.Planned;
    public bool IsActive {  get; set; }
}