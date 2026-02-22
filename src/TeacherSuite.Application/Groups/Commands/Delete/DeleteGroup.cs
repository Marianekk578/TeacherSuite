using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Groups.Commands.Delete;

public record DeleteGroupCommand(Guid Id) : IRequest<Unit>;

public class DeleteGroupHandler(IApplicationDbContext context) : IRequestHandler<DeleteGroupCommand, Unit>
{
    public async Task<Unit> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Groups.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        context.Groups.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
