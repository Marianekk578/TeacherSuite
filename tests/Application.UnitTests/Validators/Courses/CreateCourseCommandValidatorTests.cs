using FluentValidation.TestHelper;
using TeacherSuite.Application.Courses.Commands.Create;

namespace Application.UnitTests.Validators.Courses;

public class CreateCourseCommandValidatorTests
{
    private readonly CreateCourseCommandValidator _validator = new();

    private static CreateCourseCommand CreateValidCommand() => new(
        Name: "Introduction to C#",
        Description: "A beginner course",
        AgeGroupID: 1);

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
    public void Name_WhenEmpty_ShouldHaveError(string? name)
    {
        var command = CreateValidCommand() with { Name = name };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AgeGroupID_WhenNotPositive_ShouldHaveError(int ageGroupId)
    {
        var command = CreateValidCommand() with { AgeGroupID = ageGroupId };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AgeGroupID);
    }

    [Fact]
    public void AgeGroupID_WhenPositive_ShouldNotHaveError()
    {
        var command = CreateValidCommand() with { AgeGroupID = 5 };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.AgeGroupID);
    }
}
