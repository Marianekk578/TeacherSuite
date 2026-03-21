using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Students.Commands.AssignGroup;

[Authorize(Roles = AppRoles.Policies.AdminSupervisorOrTeacher)]
public record UnassignStudentFromGroupCommand(Guid StudentId, Guid GroupId) : IRequest<Unit>;

internal sealed class UnassignStudentFromGroupCommandHandler(IApplicationDbContext db) : IRequestHandler<UnassignStudentFromGroupCommand, Unit>
{
    public async Task<Unit> Handle(UnassignStudentFromGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.StudentGroups
            .FirstOrDefaultAsync(sg => sg.StudentId == request.StudentId && sg.GroupId == request.GroupId, cancellationToken);

        Guard.Against.NotFound($"{request.StudentId}-{request.GroupId}", entity);

        db.StudentGroups.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
