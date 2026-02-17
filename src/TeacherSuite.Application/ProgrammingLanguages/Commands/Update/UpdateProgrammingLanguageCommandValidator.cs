using TeacherSuite.Application.ProgrammingLanguages.Commands.Common;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Update;

public class UpdateProgrammingLanguageCommandValidator : AbstractValidator<UpdateProgrammingLanguageCommand>
{
    public UpdateProgrammingLanguageCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Programming language id is required");

        ProgrammingLanguageValidationRules.ApplyCommonRules(this,
            x => x.Name);
    }
}
