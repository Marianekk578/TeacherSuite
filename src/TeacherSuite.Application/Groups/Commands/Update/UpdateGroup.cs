using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Groups.Commands.Update;

public record UpdateGroupCommand(Guid Id, string? Name, Guid TeacherId, int CourseId) : IRequest<Unit>;

public class UpdateGroupHandler(IApplicationDbContext context) : IRequestHandler<UpdateGroupCommand, Unit>
{
    public async Task<Unit> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Groups
            .Include(g => g.GroupCourses)
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var course = await context.Courses.FindAsync(new object[] { request.CourseId }, cancellationToken);
        Guard.Against.NotFound(request.CourseId, course);

        entity.Name = request.Name;
        entity.TeacherId = request.TeacherId;
        entity.AgeGroupID = course.AgeGroupID;

        foreach (var existing in entity.GroupCourses.ToList())
        {
            context.GroupCourses.Remove(existing);
        }

        var groupCourse = new GroupCourse
        {
            GroupId = entity.Id,
            CourseId = request.CourseId,
            StartDate = DateTimeOffset.UtcNow,
            IsActive = true
        };
        context.GroupCourses.Add(groupCourse);

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
