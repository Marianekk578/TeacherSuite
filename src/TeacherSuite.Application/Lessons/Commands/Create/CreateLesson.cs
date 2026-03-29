using System.Text.Json;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Enums;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Lessons.Commands.Create;

[Authorize(AppRoles.Policies.AdminOrSupervisor)]
public record CreateLessonCommand(
    int CourseId,
    string? Title,
    string? Description,
    int DurationMinutes,
    LessonMaterialType MaterialType,
    string? MarkdownContent,
    List<string>? RequirementIcons) : IRequest<int>, ICacheInvalidationCommand
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
            MaterialType = request.MaterialType,
            MarkdownContent = request.MarkdownContent,
            RequirementIcons = request.RequirementIcons is { Count: > 0 }
                ? JsonSerializer.Serialize(request.RequirementIcons)
                : null
        };

        db.Lessons.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new LessonCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
