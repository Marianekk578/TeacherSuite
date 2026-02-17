using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public record ProgrammingLanguageCreatedEvent(ProgrammingLanguage ProgrammingLanguage) : INotification;
