using TeacherSuite.Application.Common.Exceptions;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Students.Commands.AssignGroup;

[Authorize(Roles = AppRoles.Policies.AdminSupervisorOrTeacher)]
public record UnassignStudentFromGroupCommand(Guid StudentId, Guid GroupId) : IRequest<Unit>;

internal sealed class UnassignStudentFromGroupCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IRequestHandler<UnassignStudentFromGroupCommand, Unit>
{
    public async Task<Unit> Handle(UnassignStudentFromGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.StudentGroups
            .FirstOrDefaultAsync(sg => sg.StudentId == request.StudentId && sg.GroupId == request.GroupId, cancellationToken);

        Guard.Against.NotFound($"{request.StudentId}-{request.GroupId}", entity);

        if (currentUser.IsInRole(AppRoles.Teacher)
            && !currentUser.IsInRole(AppRoles.Admin)
            && !currentUser.IsInRole(AppRoles.Supervisor))
        {
            var group = await db.Groups
                .Include(g => g.Teacher)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);

            if (group?.Teacher == null || group.Teacher.Email != currentUser.Email)
            {
                throw new ForbiddenAccessException("You can only unassign students from your own groups.");
            }
        }

        db.StudentGroups.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
