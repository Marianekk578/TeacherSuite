using TeacherSuite.Domain.Common;

namespace TeacherSuite.Domain.Entities;

public class StudentLessonAttendance : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ScheduledLessonId { get; set; }
    public ScheduledLesson? ScheduledLesson { get; set; }
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public bool IsPresent { get; set; }
}
