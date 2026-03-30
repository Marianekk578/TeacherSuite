using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class ScheduledLesson : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public Guid GroupId { get; set; }
    public Group? Group { get; set; }
    public DateTimeOffset ScheduledStart { get; set; }
    public DateTimeOffset ScheduledEnd { get; set; }
    public ICollection<StudentLessonAttendance> StudentAttendances { get; set; } = new List<StudentLessonAttendance>();
}
