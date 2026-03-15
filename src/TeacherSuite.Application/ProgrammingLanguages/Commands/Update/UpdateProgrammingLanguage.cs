using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.ProgrammingLanguages.Commands.Update;

public record UpdateProgrammingLanguageCommand(int Id, string Name, string? Label, string? Color) : IRequest<Unit>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["programming-languages"];
}

internal sealed class UpdateProgrammingLanguageCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateProgrammingLanguageCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProgrammingLanguageCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.ProgrammingLanguages.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.Label = request.Label ?? request.Name;
        entity.Color = request.Color;

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
