using FluentValidation.TestHelper;
using TeacherSuite.Application.Students.Commands.Common;
using TeacherSuite.Application.Students.Commands.Create;

namespace Application.UnitTests;

public class StudentValidationTests
{
    private readonly CreateStudentCommandValidator _validator = new();

    [Fact]
    public void Validate_StudentExactlySevenYearsOld_Succeeds()
    {
        var dob = DateTimeOffset.UtcNow.AddYears(-7);
        var command = CreateCommand(dob);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_StudentSixYearsOld_Fails()
    {
        var dob = DateTimeOffset.UtcNow.AddYears(-6);
        var command = CreateCommand(dob);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth)
            .WithErrorMessage($"Student must be at least {AgeCalculator.MinimumStudentAge} years old. Maximum birth year: {AgeCalculator.GetMaxBirthYear()}");
    }

    [Fact]
    public void Validate_StudentOneDayBeforeSeventh_Fails()
    {
        // Not yet 7 — birthday is tomorrow
        var dob = DateTimeOffset.UtcNow.AddYears(-7).AddDays(1);
        var command = CreateCommand(dob);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_StudentBornStartOfMaxYear_Succeeds()
    {
        // Born January 1st of the max birth year — should be valid as age >= 7
        var maxYear = AgeCalculator.GetMaxBirthYear();
        var dob = new DateTimeOffset(maxYear, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var command = CreateCommand(dob);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void Validate_FutureDateOfBirth_Fails()
    {
        var dob = DateTimeOffset.UtcNow.AddDays(30);
        var command = CreateCommand(dob);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    private static CreateStudentCommand CreateCommand(DateTimeOffset dob) =>
        new(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: dob,
            ContactEmail: "john@example.com",
            ContactPhone: "+48 123 456 789",
            ParentFirstName: "Jane",
            ParentLastName: "Doe",
            GroupId: null
        );
}
