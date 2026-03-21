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

internal sealed class UpdateStudentCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateStudentCommand, Unit>
{
    public async Task<Unit> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Students.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

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
