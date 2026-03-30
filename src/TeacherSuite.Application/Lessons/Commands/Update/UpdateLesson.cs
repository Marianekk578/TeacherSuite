using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Lessons.Commands.Update;

[Authorize(AppRoles.Policies.AdminOrSupervisor)]
public record UpdateLessonCommand(
    int Id,
    string? Title,
    string? Description,
    int DurationMinutes,
    List<string>? RequirementIconKeys) : IRequest<Unit>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["lessons"];
}

internal sealed class UpdateLessonCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateLessonCommand, Unit>
{
    public async Task<Unit> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Lessons.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Title = request.Title ?? string.Empty;
        entity.Description = request.Description;
        entity.DurationMinutes = request.DurationMinutes;

        var existingLinks = await db.LessonRequirementIcons
            .Where(lr => lr.LessonId == request.Id)
            .ToListAsync(cancellationToken);

        foreach (var link in existingLinks)
        {
            db.LessonRequirementIcons.Remove(link);
        }

        if (request.RequirementIconKeys is { Count: > 0 })
        {
            var icons = await db.RequirementIcons
                .Where(r => request.RequirementIconKeys.Contains(r.Key))
                .ToListAsync(cancellationToken);

            foreach (var icon in icons)
            {
                db.LessonRequirementIcons.Add(new LessonRequirementIcon
                {
                    LessonId = request.Id,
                    RequirementIconId = icon.Id
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
