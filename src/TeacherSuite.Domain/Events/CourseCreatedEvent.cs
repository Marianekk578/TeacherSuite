using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public record CourseCreatedEvent : INotification
{
    public CourseCreatedEvent(Course course)
    {
        Course = course;
    }

    public Course Course { get; }
}
