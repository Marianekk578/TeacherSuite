namespace TeacherSuite.Application.Lessons.Commands.Update;

public class UpdateLessonCommandValidator : AbstractValidator<UpdateLessonCommand>
{
    public UpdateLessonCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Lesson id is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Lesson title is required");

        RuleFor(x => x.Order)
            .GreaterThan(0)
            .WithMessage("Lesson order must be greater than 0");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than 0 minutes");
    }
}
