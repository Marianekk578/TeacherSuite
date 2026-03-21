using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Students.Commands.Delete;

[Authorize(Roles = AppRoles.Policies.AdminOrSupervisor)]
public record DeleteStudentCommand(Guid Id) : IRequest<Unit>;

internal sealed class DeleteStudentCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteStudentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Students.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        db.Students.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
