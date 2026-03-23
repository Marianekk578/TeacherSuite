namespace TeacherSuite.Application.Lessons.Commands.CreateSuggestion;

public class CreateLessonSuggestionCommandValidator : AbstractValidator<CreateLessonSuggestionCommand>
{
    public CreateLessonSuggestionCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .GreaterThan(0)
            .WithMessage("A valid lesson is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Suggestion content is required");
    }
}
