using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Delete;

public record DeleteProgrammingLanguageCommand(int Id) : IRequest<Unit>;

public class DeleteProgrammingLanguageHandler(IApplicationDbContext context) : IRequestHandler<DeleteProgrammingLanguageCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.ProgrammingLanguages.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        context.ProgrammingLanguages.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
