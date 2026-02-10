using FluentValidation.TestHelper;
using TeacherSuite.Application.Courses.Commands.Update;

namespace Application.UnitTests.Validators.Courses;

public class UpdateCourseCommandValidatorTests
{
    private readonly UpdateCourseCommandValidator _validator = new();

    private static UpdateCourseCommand CreateValidCommand() => new(
        Id: 1,
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
    [InlineData(0)]
    [InlineData(-1)]
    public void Id_WhenNotPositive_ShouldHaveError(int id)
    {
        var command = CreateValidCommand() with { Id = id };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id);
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
    public void AgeGroupID_WhenNotPositive_ShouldHaveError(int ageGroupId)
    {
        var command = CreateValidCommand() with { AgeGroupID = ageGroupId };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AgeGroupID);
    }
}
