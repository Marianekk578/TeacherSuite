using System.Linq.Expressions;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Common;

public static class ProgrammingLanguageValidationRules
{
    public static void ApplyCommonRules<T>(AbstractValidator<T> validator,
        Expression<Func<T, string?>> nameSelector)
    {
        validator.RuleFor(nameSelector)
            .NotEmpty()
            .WithMessage("Programming language name is required");
    }
}
