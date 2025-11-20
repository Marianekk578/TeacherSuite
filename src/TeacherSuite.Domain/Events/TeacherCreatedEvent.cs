using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public record TeacherCreatedEvent : INotification
{
    public TeacherCreatedEvent(Teacher teacher)
    {
        Teacher = teacher;
    }

    public Teacher Teacher { get; }
}