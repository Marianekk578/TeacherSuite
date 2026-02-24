using TeacherSuite.Application.Teachers.Commands.Common;

namespace TeacherSuite.Application.Teachers.Commands.Update;

public class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Teacher id is required");

        TeacherValidationRules.ApplyCommonRules(this,
            x => x.FirstName,
            x => x.LastName,
            x => x.Email,
            x => x.PhoneNumber,
            x => x.DateOfBirth);
    }
}