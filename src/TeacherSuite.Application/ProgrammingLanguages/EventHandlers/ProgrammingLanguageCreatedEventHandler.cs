using Microsoft.Extensions.Logging;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.EventHandlers;

public class ProgrammingLanguageCreatedEventHandler(ILogger<ProgrammingLanguageCreatedEventHandler> logger) : INotificationHandler<ProgrammingLanguageCreatedEvent>
{
    public Task Handle(ProgrammingLanguageCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Programming language created: {Name}", notification.ProgrammingLanguage.Name);

        return Task.CompletedTask;
    }
}
