using FluentValidation.TestHelper;
using TeacherSuite.Application.AgeGroups.Commands;

namespace Application.UnitTests.Validators.AgeGroups;

public class CreateAgeGroupCommandValidatorTests
{
    private readonly CreateAgeGroupCommandValidator _validator = new();

    private static CreateAgeGroupCommand CreateValidCommand() => new(
        Name: "Teenagers",
        MinAge: 13,
        MaxAge: 17);

    [Fact]
    public void ValidCommand_ShouldNotHaveErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Name_WhenEmpty_ShouldHaveError(string name)
    {
        var command = CreateValidCommand() with { Name = name };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(6)]
    public void MinAge_WhenNotGreaterThanSix_ShouldHaveError(int minAge)
    {
        var command = CreateValidCommand() with { MinAge = minAge, MaxAge = 20 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MinAge);
    }

    [Fact]
    public void MinAge_WhenGreaterThanSix_ShouldNotHaveError()
    {
        var command = CreateValidCommand() with { MinAge = 7 };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MinAge);
    }

    [Fact]
    public void MaxAge_WhenLessThanMinAge_ShouldHaveError()
    {
        var command = CreateValidCommand() with { MinAge = 10, MaxAge = 8 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MaxAge);
    }

    [Fact]
    public void MaxAge_WhenEqualToMinAge_ShouldNotHaveError()
    {
        var command = CreateValidCommand() with { MinAge = 10, MaxAge = 10 };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MaxAge);
    }

    [Fact]
    public void MaxAge_WhenGreaterThanMinAge_ShouldNotHaveError()
    {
        var command = CreateValidCommand() with { MinAge = 10, MaxAge = 15 };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.MaxAge);
    }
}
