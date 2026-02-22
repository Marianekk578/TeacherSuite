using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Groups.Commands.Create;

public record CreateGroupCommand(string? Name, Guid TeacherId) : IRequest<Guid>;

public class CreateGroupHandler : IRequestHandler<CreateGroupCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public CreateGroupHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = new Group
        {
            Name = request.Name,
            TeacherId = request.TeacherId
        };

        _context.Groups.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new GroupCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
