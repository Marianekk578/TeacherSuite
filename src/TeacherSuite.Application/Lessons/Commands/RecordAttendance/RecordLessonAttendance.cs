using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Lessons.Commands.RecordAttendance;

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record RecordLessonAttendanceCommand(int LessonId, Guid GroupId, DateTimeOffset AttendedAt) : IRequest<Guid>;

internal sealed class RecordLessonAttendanceCommandHandler(IApplicationDbContext db) : IRequestHandler<RecordLessonAttendanceCommand, Guid>
{
    public async Task<Guid> Handle(RecordLessonAttendanceCommand request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);

        Guard.Against.NotFound(request.LessonId, lesson);

        var groupAssigned = await db.GroupCourses
            .AnyAsync(gc => gc.GroupId == request.GroupId && gc.CourseId == lesson.CourseId, cancellationToken);

        if (!groupAssigned)
        {
            throw new ConflictException("The group is not assigned to this lesson's course.");
        }

        var alreadyRecorded = await db.LessonAttendances
            .AnyAsync(a => a.LessonId == request.LessonId && a.GroupId == request.GroupId, cancellationToken);

        if (alreadyRecorded)
        {
            throw new ConflictException("Attendance for this group has already been recorded for this lesson.");
        }

        var entity = new LessonAttendance
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            GroupId = request.GroupId,
            AttendedAt = request.AttendedAt
        };

        db.LessonAttendances.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
