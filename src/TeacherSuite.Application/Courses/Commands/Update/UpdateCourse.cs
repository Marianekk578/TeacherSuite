using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Courses.Commands.Update;

public record UpdateCourseCommand(int Id, string? Name, string? Description, int AgeGroupID) : IRequest<Unit>;

internal sealed class UpdateCourseCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateCourseCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Courses.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.AgeGroupID = request.AgeGroupID;

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
