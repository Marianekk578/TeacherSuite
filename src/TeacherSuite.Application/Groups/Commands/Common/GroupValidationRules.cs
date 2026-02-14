using System.Linq.Expressions;

namespace TeacherSuite.Application.Groups.Commands.Common;

public static class GroupValidationRules
{
    public static void ApplyCommonRules<T>(AbstractValidator<T> validator,
        Expression<Func<T, string?>> nameSelector,
        Expression<Func<T, Guid>> teacherIdSelector,
        Expression<Func<T, int>> ageGroupIdSelector)
    {
        validator.RuleFor(nameSelector)
            .NotEmpty()
            .WithMessage("Group name is required");

        validator.RuleFor(teacherIdSelector)
            .NotEmpty()
            .WithMessage("A teacher must be assigned to the group");

        validator.RuleFor(ageGroupIdSelector)
            .GreaterThan(0)
            .WithMessage("A valid age group is required");
    }
}
