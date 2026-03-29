using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Lessons.Commands.Reorder;

[Authorize(AppRoles.Policies.AdminOrSupervisor)]
public record ReorderLessonCommand(int LessonId, string Direction) : IRequest<Unit>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["lessons"];
}

internal sealed class ReorderLessonCommandHandler(IApplicationDbContext db) : IRequestHandler<ReorderLessonCommand, Unit>
{
    public async Task<Unit> Handle(ReorderLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);
        Guard.Against.NotFound(request.LessonId, lesson);

        var courseLessons = await db.Lessons
            .Where(l => l.CourseId == lesson.CourseId)
            .OrderBy(l => l.Order)
            .ToListAsync(cancellationToken);

        var currentIndex = courseLessons.FindIndex(l => l.Id == lesson.Id);

        int swapIndex;
        if (request.Direction == "up" && currentIndex > 0)
        {
            swapIndex = currentIndex - 1;
        }
        else if (request.Direction == "down" && currentIndex < courseLessons.Count - 1)
        {
            swapIndex = currentIndex + 1;
        }
        else
        {
            return Unit.Value;
        }

        var swapLesson = courseLessons[swapIndex];
        (lesson.Order, swapLesson.Order) = (swapLesson.Order, lesson.Order);

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
