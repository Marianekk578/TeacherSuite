using MediatR;
using Microsoft.EntityFrameworkCore;
using TeacherSuite.Application.AgeGroups.Dtos;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;
using TeacherSuite.Infrastructure.Data;

namespace TeacherSuite.Application.AgeGroups.Commands;

public class CreateAgeGroupHandler : IRequestHandler<CreateAgeGroupCommand, AgeGroupDto>
{
    private readonly ApplicationDbContext _db;
    private readonly IPublisher _publisher;

    public CreateAgeGroupHandler(ApplicationDbContext db, IPublisher publisher)
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

        _db.AgeGroups.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new AgeGroupCreatedEvent(entity), cancellationToken);

        return new AgeGroupDto(entity.Id, entity.Name, entity.MinAge, entity.MaxAge);
    }
}