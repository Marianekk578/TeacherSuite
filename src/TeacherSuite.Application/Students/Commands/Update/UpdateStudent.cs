using TeacherSuite.Application.Common.Exceptions;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Students.Commands.Update;

[Authorize(Roles = AppRoles.Policies.AdminSupervisorOrTeacher)]
public record UpdateStudentCommand(
    Guid Id,
    string FirstName,
    string LastName,
    DateTimeOffset DateOfBirth,
    string ContactEmail,
    string ContactPhone,
    string? ParentFirstName,
    string? ParentLastName) : IRequest<Unit>;

internal sealed class UpdateStudentCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IRequestHandler<UpdateStudentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Students
            .Include(s => s.StudentGroups)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        if (currentUser.IsInRole(AppRoles.Teacher)
            && !currentUser.IsInRole(AppRoles.Admin)
            && !currentUser.IsInRole(AppRoles.Supervisor))
        {
            var teacherGroupIds = await db.Groups
                .Where(g => g.Teacher != null && g.Teacher.Email == currentUser.Email)
                .Select(g => g.Id)
                .ToListAsync(cancellationToken);

            if (!entity.StudentGroups.Any(sg => teacherGroupIds.Contains(sg.GroupId)))
            {
                throw new ForbiddenAccessException("You can only update students assigned to your groups.");
            }
        }

        entity.FirstName = request.FirstName;
        entity.LastName = request.LastName;
        entity.DateOfBirth = request.DateOfBirth;
        entity.ContactEmail = request.ContactEmail;
        entity.ContactPhone = request.ContactPhone;
        entity.ParentFirstName = request.ParentFirstName;
        entity.ParentLastName = request.ParentLastName;

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
