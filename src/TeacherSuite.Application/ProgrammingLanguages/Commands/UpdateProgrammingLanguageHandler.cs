using TeacherSuite.Application.AgeGroups.Common.Interfaces;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public class UpdateProgrammingLanguageHandler : IRequestHandler<UpdateProgrammingLanguageCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly IPublisher _publisher;

    public UpdateProgrammingLanguageHandler(IApplicationDbContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<bool> Handle(UpdateProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.ProgrammingLanguages
            .FirstOrDefaultAsync(pl => pl.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        entity.Name = request.Name;
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ProgrammingLanguageUpdatedEvent(entity), cancellationToken);

        return true;
    }
}
