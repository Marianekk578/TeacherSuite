using TeacherSuite.Application.ProgrammingLanguages.Commands.Common;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Create;

public class CreateProgrammingLanguageCommandValidator : AbstractValidator<CreateProgrammingLanguageCommand>
{
    public CreateProgrammingLanguageCommandValidator()
    {
        ProgrammingLanguageValidationRules.ApplyCommonRules(this,
            x => x.Name);
    }
}
