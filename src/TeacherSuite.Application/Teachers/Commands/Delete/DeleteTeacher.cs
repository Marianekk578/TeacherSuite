using TeacherSuite.Application.AgeGroups.Common.Interfaces;

namespace TeacherSuite.Application.Teachers.Commands.Delete;

public record DeleteTeacherCommand(Guid Id) : IRequest<Unit>;

public class DeleteTeacherHandler(IApplicationDbContext context) : IRequestHandler<DeleteTeacherCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Teachers.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        context.Teachers.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
