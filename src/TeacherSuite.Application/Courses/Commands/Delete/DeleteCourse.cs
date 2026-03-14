using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Courses.Commands.Delete;

public record DeleteCourseCommand(int Id) : IRequest<Unit>;

internal sealed class DeleteCourseCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteCourseCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Courses.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var isAssignedToGroup = await context.GroupCourses
            .AnyAsync(gc => gc.CourseId == request.Id, cancellationToken);

        if (isAssignedToGroup)
        {
            throw new ConflictException("The course is assigned to a group and cannot be deleted.");
        }

        context.Courses.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
