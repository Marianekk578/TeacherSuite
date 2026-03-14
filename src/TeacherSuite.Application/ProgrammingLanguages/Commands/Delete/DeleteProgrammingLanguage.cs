using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Delete;

public record DeleteProgrammingLanguageCommand(int Id) : IRequest<Unit>;

internal sealed class DeleteProgrammingLanguageCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteProgrammingLanguageCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.ProgrammingLanguages.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var isAssigned = await context.TeacherProgrammingLanguages
            .AnyAsync(tpl => tpl.ProgrammingLanguageId == request.Id, cancellationToken);

        if (isAssigned)
        {
            throw new ConflictException("The programming language is assigned to a teacher and cannot be deleted.");
        }

        context.ProgrammingLanguages.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
