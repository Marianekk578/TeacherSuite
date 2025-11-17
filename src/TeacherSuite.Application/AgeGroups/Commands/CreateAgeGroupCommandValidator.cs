using FluentValidation;

namespace TeacherSuite.Application.AgeGroups.Commands;

public class CreateAgeGroupCommandValidator : AbstractValidator<CreateAgeGroupCommand>
{
    public CreateAgeGroupCommandValidator()
    {
        RuleFor(x => x.MinAge).GreaterThan(6);
        RuleFor(x => x.MaxAge).GreaterThanOrEqualTo(x => x.MinAge);
    }
}
