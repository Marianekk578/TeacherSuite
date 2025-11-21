namespace TeacherSuite.Application.Teachers.Commands.Create;

public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Teacher firstname is required");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Teacher lastname is required");

        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("A valid email address is required");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTimeOffset.Now)
            .NotEmpty()
            .WithMessage("A valid date of birth is required");
    }
}
