using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Teachers.Commands.AssignProgrammingLanguage;

public record UnassignProgrammingLanguageCommand(Guid TeacherId, int ProgrammingLanguageId) : IRequest<Unit>;

public class UnassignProgrammingLanguageHandler(IApplicationDbContext context) : IRequestHandler<UnassignProgrammingLanguageCommand, Unit>
{
    public async Task<Unit> Handle(UnassignProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.TeacherProgrammingLanguages
            .FirstOrDefaultAsync(tpl => tpl.TeacherId == request.TeacherId && tpl.ProgrammingLanguageId == request.ProgrammingLanguageId, cancellationToken);

        Guard.Against.NotFound($"{request.TeacherId}-{request.ProgrammingLanguageId}", entity);

        context.TeacherProgrammingLanguages.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
