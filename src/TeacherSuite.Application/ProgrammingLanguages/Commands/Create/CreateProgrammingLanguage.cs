using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Create;

public record CreateProgrammingLanguageCommand(string? Name, string? Label, string? Color) : IRequest<int>;

public class CreateProgrammingLanguageHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateProgrammingLanguageCommand, int>
{
    public async Task<int> Handle(CreateProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProgrammingLanguage
        {
            Name = request.Name,
            Label = request.Label ?? request.Name,
            Color = request.Color
        };

        db.ProgrammingLanguages.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new ProgrammingLanguageCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
