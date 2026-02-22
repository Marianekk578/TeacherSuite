using TeacherSuite.Application.Groups.Commands.Common;

namespace TeacherSuite.Application.Groups.Commands.Update;

public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Group id is required");

        GroupValidationRules.ApplyCommonRules(this,
            x => x.Name,
            x => x.TeacherId,
            x => x.AgeGroupID);
    }
}
