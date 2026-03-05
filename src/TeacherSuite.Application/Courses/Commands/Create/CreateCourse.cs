using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Courses.Commands.Create;

public record CreateCourseCommand(string? Name, string? Description, int AgeGroupID) : IRequest<int>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["courses"];
}

public class CreateCourseHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateCourseCommand, int>
{
    public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var entity = new Course
        {
            Name = request.Name,
            Description = request.Description,
            AgeGroupID = request.AgeGroupID
        };

        db.Courses.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new CourseCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
