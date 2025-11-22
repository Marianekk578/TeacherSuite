namespace TeacherSuite.Application.Teachers.Commands.Update;

public class UpdateTacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTacherCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Reacher Id is required");

        RuleFor(x => x.FirstName)
        .NotEmpty()
        .WithMessage("Teacher first name is required");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Teacher last name is required");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("A valid email address is required");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTimeOffset.Now)
            .NotEmpty()
            .WithMessage("A valid date of birth is required");
    }
}