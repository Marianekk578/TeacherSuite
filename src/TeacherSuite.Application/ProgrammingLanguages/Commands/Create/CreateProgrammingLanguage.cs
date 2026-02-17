using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Create;

public record CreateProgrammingLanguageCommand(string? Name) : IRequest<int>;

public class CreateProgrammingLanguageHandler : IRequestHandler<CreateProgrammingLanguageCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public CreateProgrammingLanguageHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<int> Handle(CreateProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = new ProgrammingLanguage
        {
            Name = request.Name
        };

        _context.ProgrammingLanguages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new ProgrammingLanguageCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
