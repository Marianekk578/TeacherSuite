using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.LessonPlan.Commands.ToggleStudentAttendance;

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record ToggleStudentAttendanceCommand(Guid ScheduledLessonId, Guid StudentId, bool IsPresent) : IRequest<Guid>;

internal sealed class ToggleStudentAttendanceCommandHandler(IApplicationDbContext db) : IRequestHandler<ToggleStudentAttendanceCommand, Guid>
{
    public async Task<Guid> Handle(ToggleStudentAttendanceCommand request, CancellationToken cancellationToken)
    {
        var scheduledLesson = await db.ScheduledLessons
            .FirstOrDefaultAsync(sl => sl.Id == request.ScheduledLessonId, cancellationToken);

        Guard.Against.NotFound(request.ScheduledLessonId, scheduledLesson);

        var studentInGroup = await db.StudentGroups
            .AnyAsync(sg => sg.StudentId == request.StudentId && sg.GroupId == scheduledLesson.GroupId, cancellationToken);

        if (!studentInGroup)
        {
            throw new ConflictException($"Student '{request.StudentId}' does not belong to the group of scheduled lesson '{request.ScheduledLessonId}'.");
        }

        var existing = await db.StudentLessonAttendances
            .FirstOrDefaultAsync(sa => sa.ScheduledLessonId == request.ScheduledLessonId
                && sa.StudentId == request.StudentId, cancellationToken);

        if (existing is not null)
        {
            existing.IsPresent = request.IsPresent;
            await db.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var entity = new StudentLessonAttendance
        {
            Id = Guid.NewGuid(),
            ScheduledLessonId = request.ScheduledLessonId,
            StudentId = request.StudentId,
            IsPresent = request.IsPresent
        };

        db.StudentLessonAttendances.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
