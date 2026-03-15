using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Groups.Commands.Update;

public record UpdateGroupCommand(Guid Id, string? Name, Guid TeacherId, int AgeGroupID) : IRequest<Unit>;

internal sealed class UpdateGroupCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateGroupCommand, Unit>
{
    public async Task<Unit> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Groups.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.TeacherId = request.TeacherId;
        entity.AgeGroupID = request.AgeGroupID;

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
