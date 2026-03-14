using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Teachers.Commands.Create;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Supervisor)]
public record CreateTeacherCommand(string FirstName, string LastName, string Email, string PhoneNumber, DateTimeOffset DateOfBirth) : IRequest<Guid>;

internal sealed class CreateTeacherCommandHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateTeacherCommand, Guid>
{
    public async Task<Guid> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        var entity = new Teacher
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth
        };

        db.Teachers.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new TeacherCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}