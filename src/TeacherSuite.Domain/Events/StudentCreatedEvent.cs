using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public record StudentCreatedEvent(Student Student) : INotification;
