namespace TeacherSuite.Application.Courses.Commands.Create;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Course name is required");

        RuleFor(x => x.AgeGroupID)
            .GreaterThan(0)
            .WithMessage("A valid age group is required");
    }
}
