using System.Linq.Expressions;

namespace TeacherSuite.Application.Students.Commands.Common;

public static class StudentValidationRules
{
    public static void ApplyCommonRules<T>(AbstractValidator<T> validator,
        Expression<Func<T, string?>> firstNameSelector,
        Expression<Func<T, string?>> lastNameSelector,
        Expression<Func<T, string>> contactEmailSelector,
        Expression<Func<T, string>> contactPhoneSelector,
        Expression<Func<T, DateTimeOffset>> dateOfBirthSelector)
    {
        validator.RuleFor(firstNameSelector)
            .NotEmpty()
            .WithMessage("Student first name is required");

        validator.RuleFor(lastNameSelector)
            .NotEmpty()
            .WithMessage("Student last name is required");

        validator.RuleFor(contactEmailSelector)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Contact email is required")
            .EmailAddress().WithMessage("A valid email address is required");

        validator.RuleFor(contactPhoneSelector)
            .NotEmpty()
            .WithMessage("Contact phone is required");

        validator.RuleFor(dateOfBirthSelector)
            .NotEmpty()
            .WithMessage("A valid date of birth is required")
            .LessThan(DateTimeOffset.Now)
            .WithMessage("Date of birth must be in the past")
            .Must(dob => AgeCalculator.CalculateAge(dob) >= AgeCalculator.MinimumStudentAge)
            .WithMessage($"Student must be at least {AgeCalculator.MinimumStudentAge} years old. Maximum birth year: {AgeCalculator.GetMaxBirthYear()}");
    }
}
