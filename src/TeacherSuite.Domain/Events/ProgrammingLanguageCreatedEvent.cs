using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public class ProgrammingLanguageCreatedEvent : INotification
{
    public ProgrammingLanguageCreatedEvent(ProgrammingLanguage programmingLanguage)
    {
        ProgrammingLanguage = programmingLanguage;
    }

    public ProgrammingLanguage ProgrammingLanguage { get; }
}
