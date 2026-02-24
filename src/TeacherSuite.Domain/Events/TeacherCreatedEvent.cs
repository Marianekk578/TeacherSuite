using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public record TeacherCreatedEvent(Teacher Teacher) : INotification;