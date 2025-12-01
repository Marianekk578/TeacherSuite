using TeacherSuite.Application.AgeGroups.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public class CreateProgrammingLanguageHandler : IRequestHandler<CreateProgrammingLanguageCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IPublisher _publisher;

    public CreateProgrammingLanguageHandler(IApplicationDbContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<int> Handle(CreateProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProgrammingLanguage
        {
            Name = request.Name
        };

        _db.ProgrammingLanguages.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ProgrammingLanguageCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
