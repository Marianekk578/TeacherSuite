using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Teachers.Commands.Delete;

public record DeleteTeacherCommand(Guid Id) : IRequest<Unit>;

internal sealed class DeleteTeacherCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteTeacherCommand, Unit>
{
    public async Task<Unit> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Teachers.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var isAssignedToGroup = await context.Groups
            .AnyAsync(g => g.TeacherId == request.Id, cancellationToken);

        if (isAssignedToGroup)
        {
            throw new ConflictException("The teacher is assigned to a group and cannot be deleted.");
        }

        context.Teachers.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
