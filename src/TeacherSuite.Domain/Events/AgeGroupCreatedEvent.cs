using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public record AgeGroupCreatedEvent(AgeGroup AgeGroup) : INotification;
