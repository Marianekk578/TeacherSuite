using FluentValidation.TestHelper;
using TeacherSuite.Application.Teachers.Commands.Create;

namespace Application.UnitTests.Validators.Teachers;

public class CreateTeacherCommandValidatorTests
{
    private readonly CreateTeacherCommandValidator _validator = new();

    private static CreateTeacherCommand CreateValidCommand() => new(
        FirstName: "John",
        LastName: "Doe",
        Email: "john.doe@example.com",
        PhoneNumber: "+1234567890",
        DateOfBirth: DateTimeOffset.Now.AddYears(-30));

    [Fact]
    public void ValidCommand_ShouldNotHaveErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void FirstName_WhenEmpty_ShouldHaveError(string? firstName)
    {
        var command = CreateValidCommand() with { FirstName = firstName };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void LastName_WhenEmpty_ShouldHaveError(string? lastName)
    {
        var command = CreateValidCommand() with { LastName = lastName };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Email_WhenEmpty_ShouldHaveError(string email)
    {
        var command = CreateValidCommand() with { Email = email };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@missing.com")]
    public void Email_WhenInvalidFormat_ShouldHaveError(string email)
    {
        var command = CreateValidCommand() with { Email = email };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Email_WhenValid_ShouldNotHaveError()
    {
        var command = CreateValidCommand() with { Email = "valid@example.com" };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void PhoneNumber_WhenEmpty_ShouldHaveError(string phone)
    {
        var command = CreateValidCommand() with { PhoneNumber = phone };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void DateOfBirth_WhenInFuture_ShouldHaveError()
    {
        var command = CreateValidCommand() with { DateOfBirth = DateTimeOffset.Now.AddYears(1) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }

    [Fact]
    public void DateOfBirth_WhenInPast_ShouldNotHaveError()
    {
        var command = CreateValidCommand() with { DateOfBirth = DateTimeOffset.Now.AddYears(-25) };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.DateOfBirth);
    }
}
