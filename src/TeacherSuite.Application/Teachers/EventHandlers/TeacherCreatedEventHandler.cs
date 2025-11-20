using Microsoft.Extensions.Logging;

namespace TeacherSuite.Application.Teachers.EventHandlers;

public class TeacherCreatedEventHandler(ILogger<TeacherCreatedEventHandler> logger) : INotificationHandler<TeacherCreatedEvent>
{
    public Task Handle(TeacherCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Teacher created: {Firstname} {Lastname}", notification.Teacher.FirstName, notification.Teacher.LastName);

        return Task.CompletedTask;
    }
}