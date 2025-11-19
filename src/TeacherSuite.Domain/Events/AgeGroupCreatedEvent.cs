using MediatR;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Events;

public class AgeGroupCreatedEvent : INotification
{
    public AgeGroupCreatedEvent(AgeGroup ageGroup)
    {
        AgeGroup = ageGroup;
    }

    public AgeGroup AgeGroup { get; }
}
