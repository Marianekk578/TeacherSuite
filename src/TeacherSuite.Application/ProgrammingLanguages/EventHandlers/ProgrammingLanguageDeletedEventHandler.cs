using MediatR;
using Microsoft.Extensions.Logging;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.ProgrammingLanguages.EventHandlers;

public class ProgrammingLanguageDeletedEventHandler(ILogger<ProgrammingLanguageDeletedEventHandler> logger) : INotificationHandler<ProgrammingLanguageDeletedEvent>
{
    public Task Handle(ProgrammingLanguageDeletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Programming language deleted: {ProgrammingLanguageId}", notification.ProgrammingLanguageId);

        return Task.CompletedTask;
    }
}
