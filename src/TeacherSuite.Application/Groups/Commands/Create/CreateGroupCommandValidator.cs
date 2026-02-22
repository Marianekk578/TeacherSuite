using TeacherSuite.Application.Groups.Commands.Common;

namespace TeacherSuite.Application.Groups.Commands.Create;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        GroupValidationRules.ApplyCommonRules(this,
            x => x.Name,
            x => x.TeacherId,
            x => x.AgeGroupID);
    }
}
