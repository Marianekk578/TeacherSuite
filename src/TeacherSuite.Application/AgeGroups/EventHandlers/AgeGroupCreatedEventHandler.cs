using Microsoft.Extensions.Logging;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.AgeGroups.EventHandlers;

public class AgeGroupCreatedEventHandler(ILogger<AgeGroupCreatedEventHandler> logger) : INotificationHandler<AgeGroupCreatedEvent>
{
    public Task Handle(AgeGroupCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Age group created: {AgeGroupId}", notification.AgeGroup.Id);

        return Task.CompletedTask;
    }
}
