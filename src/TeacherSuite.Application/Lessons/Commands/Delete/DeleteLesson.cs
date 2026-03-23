using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Lessons.Commands.Delete;

[Authorize(AppRoles.Policies.AdminOrSupervisor)]
public record DeleteLessonCommand(int Id) : IRequest<Unit>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["lessons"];
}

internal sealed class DeleteLessonCommandHandler(IApplicationDbContext db, IFileStorageService fileStorage) : IRequestHandler<DeleteLessonCommand, Unit>
{
    public async Task<Unit> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Lessons.FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        if (!string.IsNullOrEmpty(entity.MaterialStorageKey))
        {
            await fileStorage.DeleteAsync(entity.MaterialStorageKey, cancellationToken);
        }

        db.Lessons.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
