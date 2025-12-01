using TeacherSuite.Application.AgeGroups.Common.Interfaces;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public class DeleteProgrammingLanguageHandler : IRequestHandler<DeleteProgrammingLanguageCommand, bool>
{
    private readonly IApplicationDbContext _db;
    private readonly IPublisher _publisher;

    public DeleteProgrammingLanguageHandler(IApplicationDbContext db, IPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<bool> Handle(DeleteProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = await _db.ProgrammingLanguages
            .FirstOrDefaultAsync(pl => pl.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        _db.ProgrammingLanguages.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ProgrammingLanguageDeletedEvent(entity.Id), cancellationToken);

        return true;
    }
}
