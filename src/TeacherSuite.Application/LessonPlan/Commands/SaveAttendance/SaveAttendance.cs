using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.LessonPlan.Commands.SaveAttendance;

public record StudentAttendanceEntry(Guid StudentId, bool IsPresent);

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record SaveAttendanceCommand(Guid ScheduledLessonId, List<StudentAttendanceEntry> Attendances) : IRequest;

internal sealed class SaveAttendanceCommandHandler(IApplicationDbContext db) : IRequestHandler<SaveAttendanceCommand>
{
    public async Task Handle(SaveAttendanceCommand request, CancellationToken cancellationToken)
    {
        var scheduledLesson = await db.ScheduledLessons
            .FirstOrDefaultAsync(sl => sl.Id == request.ScheduledLessonId, cancellationToken);

        Guard.Against.NotFound(request.ScheduledLessonId, scheduledLesson);

        var groupStudentIds = await db.StudentGroups
            .Where(sg => sg.GroupId == scheduledLesson.GroupId)
            .Select(sg => sg.StudentId)
            .ToHashSetAsync(cancellationToken);

        var existingAttendances = await db.StudentLessonAttendances
            .Where(sa => sa.ScheduledLessonId == request.ScheduledLessonId)
            .ToDictionaryAsync(sa => sa.StudentId, cancellationToken);

        foreach (var entry in request.Attendances)
        {
            if (!groupStudentIds.Contains(entry.StudentId))
            {
                throw new ConflictException(
                    $"Student '{entry.StudentId}' does not belong to the group of scheduled lesson '{request.ScheduledLessonId}'.");
            }

            if (existingAttendances.TryGetValue(entry.StudentId, out var existing))
            {
                existing.IsPresent = entry.IsPresent;
            }
            else
            {
                db.StudentLessonAttendances.Add(new StudentLessonAttendance
                {
                    Id = Guid.NewGuid(),
                    ScheduledLessonId = request.ScheduledLessonId,
                    StudentId = entry.StudentId,
                    IsPresent = entry.IsPresent,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
