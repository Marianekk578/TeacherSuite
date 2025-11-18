using MediatR;
using TeacherSuite.Application.AgeGroups.Dtos;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;
using TeacherSuite.Domain.Interfaces;

namespace TeacherSuite.Application.AgeGroups.Commands;

public class CreateAgeGroupHandler : IRequestHandler<CreateAgeGroupCommand, AgeGroupDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IPublisher _publisher;

    public CreateAgeGroupHandler(IApplicationDbContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<AgeGroupDto> Handle(CreateAgeGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = new AgeGroup
        {
            Name = request.Name,
            MinAge = request.MinAge,
            MaxAge = request.MaxAge
        };

        _db.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new AgeGroupCreatedEvent(entity), cancellationToken);

        return new AgeGroupDto(entity.Id, entity.Name, entity.MinAge, entity.MaxAge);
    }
}