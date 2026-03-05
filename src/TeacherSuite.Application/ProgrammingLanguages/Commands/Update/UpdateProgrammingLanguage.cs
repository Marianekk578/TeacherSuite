using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Update;

public record UpdateProgrammingLanguageCommand(int Id, string? Name) : IRequest<Unit>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["programming-languages"];
}

public class UpdateProgrammingLanguageHandler(IApplicationDbContext context) : IRequestHandler<UpdateProgrammingLanguageCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.ProgrammingLanguages.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
