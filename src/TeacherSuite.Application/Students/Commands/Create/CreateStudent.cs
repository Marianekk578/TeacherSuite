using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Students.Commands.Create;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Supervisor + "," + AppRoles.Teacher)]
public record CreateStudentCommand(
    string FirstName,
    string LastName,
    DateTimeOffset DateOfBirth,
    string ContactEmail,
    string ContactPhone,
    string? ParentFirstName,
    string? ParentLastName,
    Guid? GroupId) : IRequest<Guid>;

internal sealed class CreateStudentCommandHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateStudentCommand, Guid>
{
    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        if (request.GroupId.HasValue)
        {
            var group = await db.Groups
                .Include(g => g.AgeGroup)
                .FirstOrDefaultAsync(g => g.Id == request.GroupId.Value, cancellationToken);

            Guard.Against.NotFound(request.GroupId.Value, group);

            if (group.AgeGroup != null)
            {
                var age = CalculateAge(request.DateOfBirth);
                if (age < group.AgeGroup.MinAge || age > group.AgeGroup.MaxAge)
                {
                    throw new Application.Common.ValidationException(new[]
                    {
                        new FluentValidation.Results.ValidationFailure("GroupId",
                            $"Student age ({age}) does not match the group's age range ({group.AgeGroup.MinAge}-{group.AgeGroup.MaxAge}).")
                    });
                }
            }
        }

        var entity = new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            ParentFirstName = request.ParentFirstName,
            ParentLastName = request.ParentLastName
        };

        db.Students.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        if (request.GroupId.HasValue)
        {
            db.StudentGroups.Add(new StudentGroup
            {
                StudentId = entity.Id,
                GroupId = request.GroupId.Value
            });
            await db.SaveChangesAsync(cancellationToken);
        }

        await publisher.Publish(new StudentCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }

    private static int CalculateAge(DateTimeOffset dateOfBirth)
    {
        var today = DateTimeOffset.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }
}
