namespace TeacherSuite.Application.LessonPlan.Commands.ToggleStudentAttendance;

public class ToggleStudentAttendanceCommandValidator : AbstractValidator<ToggleStudentAttendanceCommand>
{
    public ToggleStudentAttendanceCommandValidator()
    {
        RuleFor(x => x.ScheduledLessonId)
            .NotEmpty()
            .WithMessage("A valid scheduled lesson is required");

        RuleFor(x => x.StudentId)
            .NotEmpty()
            .WithMessage("A valid student is required");
    }
}
