using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public record CourseCreatedEvent(Course Course) : INotification;
