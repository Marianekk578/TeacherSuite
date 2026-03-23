using TeacherSuite.Application.Students.Commands.Common;

namespace TeacherSuite.Application.Students.Commands.Update;

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Student id is required");

        StudentValidationRules.ApplyCommonRules(this,
            x => x.FirstName,
            x => x.LastName,
            x => x.ContactEmail,
            x => x.ContactPhone,
            x => x.DateOfBirth);
    }
}
