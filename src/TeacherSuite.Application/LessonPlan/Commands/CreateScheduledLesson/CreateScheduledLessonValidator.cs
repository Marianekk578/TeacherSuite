namespace TeacherSuite.Application.LessonPlan.Commands.CreateScheduledLesson;

public class CreateScheduledLessonCommandValidator : AbstractValidator<CreateScheduledLessonCommand>
{
    public CreateScheduledLessonCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0)
            .WithMessage("A valid lesson is required");

        RuleFor(x => x.GroupId)
            .NotEmpty()
            .WithMessage("A valid group is required");

        RuleFor(x => x.ScheduledStart)
            .NotEqual(default(DateTimeOffset))
            .WithMessage("A valid scheduled start time is required");
    }
}
