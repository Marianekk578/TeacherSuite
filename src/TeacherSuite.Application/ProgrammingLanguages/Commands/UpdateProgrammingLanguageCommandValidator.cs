namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public class UpdateProgrammingLanguageCommandValidator : AbstractValidator<UpdateProgrammingLanguageCommand>
{
    public UpdateProgrammingLanguageCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Programming language ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Programming language name is required")
            .MaximumLength(100)
            .WithMessage("Programming language name must not exceed 100 characters");
    }
}
