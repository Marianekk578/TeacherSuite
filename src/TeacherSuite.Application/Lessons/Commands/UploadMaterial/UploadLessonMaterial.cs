using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Lessons.Commands.UploadMaterial;

[Authorize(AppRoles.Policies.AdminOrSupervisor)]
public record UploadLessonMaterialCommand(int LessonId, string FileName, Stream FileContent) : IRequest<Unit>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["lessons"];
}

internal sealed class UploadLessonMaterialCommandHandler(IApplicationDbContext db, IFileStorageService fileStorage) : IRequestHandler<UploadLessonMaterialCommand, Unit>
{
    public async Task<Unit> Handle(UploadLessonMaterialCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);

        Guard.Against.NotFound(request.LessonId, entity);

        if (string.IsNullOrEmpty(entity.AlbumId))
        {
            entity.AlbumId = await fileStorage.CreateAlbumAsync(entity.Title, cancellationToken);
        }

        var storageKey = await fileStorage.UploadAsync(request.FileName, request.FileContent, cancellationToken);

        await fileStorage.AddFileToAlbumAsync(entity.AlbumId, storageKey, cancellationToken);

        entity.MaterialFileName = request.FileName;
        entity.MaterialStorageKey = storageKey;
        entity.MaterialType = LessonMaterialType.Word;

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
