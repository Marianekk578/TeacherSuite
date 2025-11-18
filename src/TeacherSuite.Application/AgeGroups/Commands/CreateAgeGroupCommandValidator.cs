using FluentValidation;

namespace TeacherSuite.Application.AgeGroups.Commands;

public class CreateAgeGroupCommandValidator : AbstractValidator<CreateAgeGroupCommand>
{
    private const int MinimumAllowedAge = 6;

    public CreateAgeGroupCommandValidator()
    {
        RuleFor(x => x.MinAge)
            .GreaterThan(MinimumAllowedAge)
            .WithMessage($"Minimum age must be greater than {MinimumAllowedAge}");

        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge)
            .WithMessage("Maximum age can't be greater than minimum.");
    }
}