using TeacherSuite.Application.Teachers.Commands.Common;

namespace TeacherSuite.Application.Teachers.Commands.Create;

public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        TeacherValidationRules.ApplyCommonRules(this,
            x => x.FirstName,
            x => x.LastName,
            x => x.Email,
            x => x.PhoneNumber,
            x => x.DateOfBirth);
    }
}
