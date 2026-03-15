using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Groups.Commands.AssignCourse;

public record UnassignCourseFromGroupCommand(Guid GroupId, int CourseId) : IRequest<Unit>;

internal sealed class UnassignCourseFromGroupCommandHandler(IApplicationDbContext context) : IRequestHandler<UnassignCourseFromGroupCommand, Unit>
{
    public async Task<Unit> Handle(UnassignCourseFromGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.GroupCourses
            .FirstOrDefaultAsync(gc => gc.GroupId == request.GroupId && gc.CourseId == request.CourseId, cancellationToken);

        Guard.Against.NotFound($"{request.GroupId}-{request.CourseId}", entity);

        context.GroupCourses.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
