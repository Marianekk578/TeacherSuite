using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Groups.Commands.Create;

public record CreateGroupCommand(string? Name, Guid TeacherId, int CourseId) : IRequest<Guid>;

public class CreateGroupHandler : IRequestHandler<CreateGroupCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public CreateGroupHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses.FindAsync(new object[] { request.CourseId }, cancellationToken);
        Guard.Against.NotFound(request.CourseId, course);

        var entity = new Group
        {
            Name = request.Name,
            TeacherId = request.TeacherId,
            AgeGroupID = course.AgeGroupID
        };

        _context.Groups.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        var groupCourse = new GroupCourse
        {
            GroupId = entity.Id,
            CourseId = request.CourseId,
            StartDate = DateTimeOffset.UtcNow,
            IsActive = true
        };

        _context.GroupCourses.Add(groupCourse);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new GroupCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
