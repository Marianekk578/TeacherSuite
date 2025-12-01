using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public class ProgrammingLanguageUpdatedEvent : INotification
{
    public ProgrammingLanguageUpdatedEvent(ProgrammingLanguage programmingLanguage)
    {
        ProgrammingLanguage = programmingLanguage;
    }

    public ProgrammingLanguage ProgrammingLanguage { get; }
}
