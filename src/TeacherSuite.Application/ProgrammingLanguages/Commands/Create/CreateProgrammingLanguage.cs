using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Create;

public record CreateProgrammingLanguageCommand(string? Name) : IRequest<int>;

public class CreateProgrammingLanguageHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateProgrammingLanguageCommand, int>
{
    public async Task<int> Handle(CreateProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProgrammingLanguage
        {
            Name = request.Name
        };

        db.ProgrammingLanguages.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new ProgrammingLanguageCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
