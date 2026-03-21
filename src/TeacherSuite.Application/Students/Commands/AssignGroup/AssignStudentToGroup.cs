using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Students.Commands.Common;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Students.Commands.AssignGroup;

[Authorize(Roles = AppRoles.Policies.AdminSupervisorOrTeacher)]
public record AssignStudentToGroupCommand(Guid StudentId, Guid GroupId) : IRequest<Unit>;

internal sealed class AssignStudentToGroupCommandHandler(IApplicationDbContext db) : IRequestHandler<AssignStudentToGroupCommand, Unit>
{
    public async Task<Unit> Handle(AssignStudentToGroupCommand request, CancellationToken cancellationToken)
    {
        var student = await db.Students.FindAsync(new object[] { request.StudentId }, cancellationToken);
        Guard.Against.NotFound(request.StudentId, student);

        var group = await db.Groups
            .Include(g => g.AgeGroup)
            .FirstOrDefaultAsync(g => g.Id == request.GroupId, cancellationToken);
        Guard.Against.NotFound(request.GroupId, group);

        if (group.AgeGroup != null)
        {
            var age = AgeCalculator.CalculateAge(student.DateOfBirth);
            if (age < group.AgeGroup.MinAge || age > group.AgeGroup.MaxAge)
            {
                throw new ConflictException(
                    $"Student age ({age}) does not match the group's age range ({group.AgeGroup.MinAge}-{group.AgeGroup.MaxAge}).");
            }
        }

        var exists = await db.StudentGroups
            .AnyAsync(sg => sg.StudentId == request.StudentId && sg.GroupId == request.GroupId, cancellationToken);

        if (exists)
        {
            throw new ConflictException("Student is already assigned to this group.");
        }

        db.StudentGroups.Add(new StudentGroup
        {
            StudentId = request.StudentId,
            GroupId = request.GroupId
        });

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
