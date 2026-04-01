using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class LessonAttendance : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public Guid GroupId { get; set; }
    public Group? Group { get; set; }
    public DateTimeOffset AttendedAt { get; set; }
}
