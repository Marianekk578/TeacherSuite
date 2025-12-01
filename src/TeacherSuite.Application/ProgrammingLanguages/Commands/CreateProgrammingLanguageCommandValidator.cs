namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public class CreateProgrammingLanguageCommandValidator : AbstractValidator<CreateProgrammingLanguageCommand>
{
    public CreateProgrammingLanguageCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Programming language name is required")
            .MaximumLength(100)
            .WithMessage("Programming language name must not exceed 100 characters");
    }
}
