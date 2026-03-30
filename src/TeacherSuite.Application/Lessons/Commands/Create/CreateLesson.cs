using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Lessons.Commands.Create;

[Authorize(AppRoles.Policies.AdminOrSupervisor)]
public record CreateLessonCommand(
    int CourseId,
    string? Title,
    string? Description,
    int DurationMinutes,
    List<string>? RequirementIconKeys) : IRequest<int>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["lessons"];
}

internal sealed class CreateLessonCommandHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateLessonCommand, int>
{
    public async Task<int> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        var maxOrder = await db.Lessons
            .Where(l => l.CourseId == request.CourseId)
            .Select(l => (int?)l.Order)
            .MaxAsync(cancellationToken) ?? 0;

        var entity = new Lesson
        {
            CourseId = request.CourseId,
            Title = request.Title ?? string.Empty,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes,
            Order = maxOrder + 1,
        };

        db.Lessons.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        if (request.RequirementIconKeys is { Count: > 0 })
        {
            var icons = await db.RequirementIcons
                .Where(r => request.RequirementIconKeys.Contains(r.Key))
                .ToListAsync(cancellationToken);

            foreach (var icon in icons)
            {
                db.LessonRequirementIcons.Add(new LessonRequirementIcon
                {
                    LessonId = entity.Id,
                    RequirementIconId = icon.Id
                });
            }
            await db.SaveChangesAsync(cancellationToken);
        }

        await publisher.Publish(new LessonCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
