using Microsoft.Extensions.Logging;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Groups.EventHandlers;

public class GroupCreatedEventHandler(ILogger<GroupCreatedEventHandler> logger) : INotificationHandler<GroupCreatedEvent>
{
    public Task Handle(GroupCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Group created: {Name}", notification.Group.Name);
        return Task.CompletedTask;
    }
}
