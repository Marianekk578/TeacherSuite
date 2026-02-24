using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Groups.Commands.Create;

public record CreateGroupCommand(string? Name, Guid TeacherId, int AgeGroupID) : IRequest<Guid>;

public class CreateGroupHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateGroupCommand, Guid>
{
    public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = new Group
        {
            Name = request.Name,
            TeacherId = request.TeacherId,
            AgeGroupID = request.AgeGroupID
        };

        db.Groups.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new GroupCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
