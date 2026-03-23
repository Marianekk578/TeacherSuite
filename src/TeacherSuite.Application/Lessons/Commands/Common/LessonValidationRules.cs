using System.Linq.Expressions;

namespace TeacherSuite.Application.Lessons.Commands.Common;

public static class LessonValidationRules
{
    public static void ApplyCommonRules<T>(AbstractValidator<T> validator,
        Expression<Func<T, string?>> titleSelector,
        Expression<Func<T, int>> courseIdSelector,
        Expression<Func<T, int>> orderSelector,
        Expression<Func<T, int>> durationMinutesSelector)
    {
        validator.RuleFor(titleSelector)
            .NotEmpty()
            .WithMessage("Lesson title is required");

        validator.RuleFor(courseIdSelector)
            .GreaterThan(0)
            .WithMessage("A valid course is required");

        validator.RuleFor(orderSelector)
            .GreaterThan(0)
            .WithMessage("Lesson order must be greater than 0");

        validator.RuleFor(durationMinutesSelector)
            .GreaterThan(0)
            .WithMessage("Duration must be greater than 0 minutes");
    }
}
