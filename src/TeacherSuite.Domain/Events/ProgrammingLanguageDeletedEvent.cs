using MediatR;

namespace TeacherSuite.Domain.Events;

public class ProgrammingLanguageDeletedEvent : INotification
{
    public ProgrammingLanguageDeletedEvent(int programmingLanguageId)
    {
        ProgrammingLanguageId = programmingLanguageId;
    }

    public int ProgrammingLanguageId { get; }
}
