using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.AgeGroups.Commands;

public record CreateAgeGroupCommand(string Name, string? Label, int MinAge, int MaxAge) : IRequest<int>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["agegroups"];
}

internal sealed class CreateAgeGroupCommandHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateAgeGroupCommand, int>
{
    public async Task<int> Handle(CreateAgeGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = new AgeGroup
        {
            Name = request.Name,
            Label = request.Label ?? request.Name,
            MinAge = request.MinAge,
            MaxAge = request.MaxAge
        };

        db.AgeGroups.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new AgeGroupCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}