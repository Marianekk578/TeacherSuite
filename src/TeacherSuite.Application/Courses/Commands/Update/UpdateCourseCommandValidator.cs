namespace TeacherSuite.Application.Courses.Commands.Update;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Course id is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Course name is required");

        RuleFor(x => x.AgeGroupID)
            .GreaterThan(0)
            .WithMessage("A valid age group is required");
    }
}
