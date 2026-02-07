using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.AgeGroups.Commands;

public record CreateAgeGroupCommand(string Name, int MinAge, int MaxAge) : IRequest<int>;

public class CreateAgeGroupHandler : IRequestHandler<CreateAgeGroupCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public CreateAgeGroupHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<int> Handle(CreateAgeGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = new AgeGroup
        {
            Name = request.Name,
            MinAge = request.MinAge,
            MaxAge = request.MaxAge
        };

        _context.AgeGroups.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new AgeGroupCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}