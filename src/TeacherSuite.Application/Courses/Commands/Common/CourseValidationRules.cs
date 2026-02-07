using System.Linq.Expressions;

namespace TeacherSuite.Application.Courses.Commands.Common;

public static class CourseValidationRules
{
    public static void ApplyCommonRules<T>(AbstractValidator<T> validator,
        Expression<Func<T, string?>> nameSelector,
        Expression<Func<T, int>> ageGroupIdSelector)
    {
        validator.RuleFor(nameSelector)
            .NotEmpty()
            .WithMessage("Course name is required");

        validator.RuleFor(ageGroupIdSelector)
            .GreaterThan(0)
            .WithMessage("A valid age group is required");
    }
}
