using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Courses.Commands.Delete;

public record DeleteCourseCommand(int Id) : IRequest<Unit>;

public class DeleteCourseHandler(IApplicationDbContext context) : IRequestHandler<DeleteCourseCommand, Unit>
{
    public async Task<Unit> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Courses.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        context.Courses.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
