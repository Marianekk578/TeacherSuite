using TeacherSuite.Application.Students.Commands.Common;

namespace TeacherSuite.Application.Students.Commands.Create;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        StudentValidationRules.ApplyCommonRules(this,
            x => x.FirstName,
            x => x.LastName,
            x => x.ContactEmail,
            x => x.ContactPhone,
            x => x.DateOfBirth);
    }
}
