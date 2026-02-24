using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Teachers.Commands.AssignProgrammingLanguage;

public record AssignProgrammingLanguageCommand(Guid TeacherId, int ProgrammingLanguageId) : IRequest<Unit>;

public class AssignProgrammingLanguageHandler(IApplicationDbContext context) : IRequestHandler<AssignProgrammingLanguageCommand, Unit>
{
    public async Task<Unit> Handle(AssignProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var teacher = await context.Teachers.FindAsync(new object[] { request.TeacherId }, cancellationToken);
        Guard.Against.NotFound(request.TeacherId, teacher);

        var programmingLanguage = await context.ProgrammingLanguages.FindAsync(new object[] { request.ProgrammingLanguageId }, cancellationToken);
        Guard.Against.NotFound(request.ProgrammingLanguageId, programmingLanguage);

        var exists = await context.TeacherProgrammingLanguages
            .AnyAsync(tpl => tpl.TeacherId == request.TeacherId && tpl.ProgrammingLanguageId == request.ProgrammingLanguageId, cancellationToken);

        if (!exists)
        {
            context.TeacherProgrammingLanguages.Add(new TeacherProgrammingLanguage
            {
                TeacherId = request.TeacherId,
                ProgrammingLanguageId = request.ProgrammingLanguageId
            });

            await context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
