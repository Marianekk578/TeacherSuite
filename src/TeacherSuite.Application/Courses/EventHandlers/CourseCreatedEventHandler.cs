using Microsoft.Extensions.Logging;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Courses.EventHandlers;

public class CourseCreatedEventHandler(ILogger<CourseCreatedEventHandler> logger) : INotificationHandler<CourseCreatedEvent>
{
    public Task Handle(CourseCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Course created: {Name}", notification.Course.Name);

        return Task.CompletedTask;
    }
}
