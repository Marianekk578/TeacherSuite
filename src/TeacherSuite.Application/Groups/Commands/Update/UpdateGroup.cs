using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Groups.Commands.Update;

public record UpdateGroupCommand(Guid Id, string? Name, Guid TeacherId) : IRequest<Unit>;

public class UpdateGroupHandler(IApplicationDbContext context) : IRequestHandler<UpdateGroupCommand, Unit>
{
    public async Task<Unit> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Groups.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.TeacherId = request.TeacherId;

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
