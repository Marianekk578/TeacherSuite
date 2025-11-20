using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Teachers.EventHandlers;

public record TeacherCreatedEvent : INotification
{
    public TeacherCreatedEvent(Teacher teacher)
    {
        Teacher = teacher;
    }

    public Teacher Teacher { get; }
}