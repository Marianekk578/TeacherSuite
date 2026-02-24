using TeacherSuite.Application.Courses.Commands.Common;

namespace TeacherSuite.Application.Courses.Commands.Update;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Course id is required");

        CourseValidationRules.ApplyCommonRules(this,
            x => x.Name,
            x => x.AgeGroupID);
    }
}
