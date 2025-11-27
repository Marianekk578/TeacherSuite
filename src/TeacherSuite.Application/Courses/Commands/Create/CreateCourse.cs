using TeacherSuite.Application.AgeGroups.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Courses.Commands.Create;

public record CreateCourseCommand(string? Name, string? Description, int AgeGroupID) : IRequest<int>;

public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublisher _publisher;

    public CreateCourseHandler(IApplicationDbContext context, IPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var entity = new Course
        {
            Name = request.Name,
            Description = request.Description,
            AgeGroupID = request.AgeGroupID
        };

        _context.Courses.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new CourseCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
