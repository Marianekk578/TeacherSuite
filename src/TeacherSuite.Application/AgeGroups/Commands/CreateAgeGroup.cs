using TeacherSuite.Application.AgeGroups.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.AgeGroups.Commands;

public record CreateAgeGroupCommand(string Name, int MinAge, int MaxAge) : IRequest<int>;

public class CreateAgeGroupHandler : IRequestHandler<CreateAgeGroupCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IPublisher _publisher;

    public CreateAgeGroupHandler(IApplicationDbContext db, IPublisher publisher)
    {
        _db = db;
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

        _db.AgeGroups.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new AgeGroupCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}