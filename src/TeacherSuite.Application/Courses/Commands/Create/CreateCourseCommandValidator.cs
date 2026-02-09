using TeacherSuite.Application.Courses.Commands.Common;

namespace TeacherSuite.Application.Courses.Commands.Create;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        CourseValidationRules.ApplyCommonRules(this,
            x => x.Name,
            x => x.AgeGroupID);
    }
}
