using MediatR;
using Microsoft.Extensions.Logging;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.EventHandlers;

public class ProgrammingLanguageUpdatedEventHandler(ILogger<ProgrammingLanguageUpdatedEventHandler> logger) : INotificationHandler<ProgrammingLanguageUpdatedEvent>
{
    public Task Handle(ProgrammingLanguageUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Programming language updated: {ProgrammingLanguageId}", notification.ProgrammingLanguage.Id);

        return Task.CompletedTask;
    }
}
