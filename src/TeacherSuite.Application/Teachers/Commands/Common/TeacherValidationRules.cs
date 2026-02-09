using System.Linq.Expressions;

namespace TeacherSuite.Application.Teachers.Commands.Common;

public static class TeacherValidationRules
{
    public static void ApplyCommonRules<T>(AbstractValidator<T> validator,
        Expression<Func<T, string?>> firstNameSelector,
        Expression<Func<T, string?>> lastNameSelector,
        Expression<Func<T, string>> emailSelector,
        Expression<Func<T, string>> phoneNumberSelector,
        Expression<Func<T, DateTimeOffset>> dateOfBirthSelector)
    {
        validator.RuleFor(firstNameSelector)
            .NotEmpty()
            .WithMessage("Teacher first name is required");

        validator.RuleFor(lastNameSelector)
            .NotEmpty()
            .WithMessage("Teacher last name is required");

        validator.RuleFor(emailSelector)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email address is required")
            .EmailAddress().WithMessage("A valid email address is required");

        validator.RuleFor(phoneNumberSelector)
            .NotEmpty()
            .WithMessage("Phone number is required");

        validator.RuleFor(dateOfBirthSelector)
            .LessThan(DateTimeOffset.Now)
            .NotEmpty()
            .WithMessage("A valid date of birth is required");
    }
}
