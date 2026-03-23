using Microsoft.Extensions.Logging;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Students.EventHandlers;

public class StudentCreatedEventHandler(ILogger<StudentCreatedEventHandler> logger) : INotificationHandler<StudentCreatedEvent>
{
    public Task Handle(StudentCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Student created: {FirstName} {LastName}", notification.Student.FirstName, notification.Student.LastName);
        return Task.CompletedTask;
    }
}
