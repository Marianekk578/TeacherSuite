using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.LessonPlan.Commands.CreateScheduledLesson;

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record CreateScheduledLessonCommand(int LessonId, Guid GroupId, DateTimeOffset ScheduledStart) : IRequest<Guid>;

internal sealed class CreateScheduledLessonCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateScheduledLessonCommand, Guid>
{
    public async Task<Guid> Handle(CreateScheduledLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);
        Guard.Against.NotFound(request.LessonId, lesson);

        var groupAssigned = await db.GroupCourses
            .AnyAsync(gc => gc.GroupId == request.GroupId && gc.CourseId == lesson.CourseId, cancellationToken);

        if (!groupAssigned)
        {
            throw new ConflictException("The group is not assigned to this lesson's course.");
        }

        var overlapping = await db.ScheduledLessons
            .AnyAsync(sl => sl.LessonId == request.LessonId
                && sl.GroupId == request.GroupId
                && sl.ScheduledStart == request.ScheduledStart, cancellationToken);

        if (overlapping)
        {
            throw new ConflictException("This lesson is already scheduled for this group at the specified time.");
        }

        var entity = new ScheduledLesson
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            GroupId = request.GroupId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledStart.AddMinutes(lesson.DurationMinutes)
        };

        db.ScheduledLessons.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
