using System.Text.Json;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Lessons.Commands.Update;

[Authorize(AppRoles.Policies.AdminOrSupervisor)]
public record UpdateLessonCommand(
    int Id,
    string? Title,
    string? Description,
    int DurationMinutes,
    int Order,
    LessonMaterialType MaterialType,
    string? MarkdownContent,
    List<string>? RequirementIcons) : IRequest<Unit>, ICacheInvalidationCommand
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
        entity.Order = request.Order;
        entity.MaterialType = request.MaterialType;
        entity.MarkdownContent = request.MarkdownContent;
        entity.RequirementIcons = request.RequirementIcons is { Count: > 0 }
            ? JsonSerializer.Serialize(request.RequirementIcons)
            : null;

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
