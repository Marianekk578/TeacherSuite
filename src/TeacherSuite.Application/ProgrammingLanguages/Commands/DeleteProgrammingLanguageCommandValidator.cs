namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public class DeleteProgrammingLanguageCommandValidator : AbstractValidator<DeleteProgrammingLanguageCommand>
{
    public DeleteProgrammingLanguageCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Programming language ID must be greater than 0");
    }
}
