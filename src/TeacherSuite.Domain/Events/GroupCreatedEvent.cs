using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public record GroupCreatedEvent(Group Group) : INotification;
