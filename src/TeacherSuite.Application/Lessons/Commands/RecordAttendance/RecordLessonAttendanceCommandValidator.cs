namespace TeacherSuite.Application.Lessons.Commands.RecordAttendance;

public class RecordLessonAttendanceCommandValidator : AbstractValidator<RecordLessonAttendanceCommand>
{
    public RecordLessonAttendanceCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0)
            .WithMessage("A valid lesson is required");

        RuleFor(x => x.GroupId)
            .NotEmpty()
            .WithMessage("A valid group is required");

        RuleFor(x => x.AttendedAt)
            .NotEqual(default(DateTimeOffset))
            .WithMessage("Attendance date is required");
    }
}
