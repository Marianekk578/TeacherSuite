using FluentValidation.Results;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Lessons.Commands.UploadMaterial;

[Authorize(AppRoles.Policies.AdminOrSupervisor)]
public record UploadLessonMaterialCommand(int LessonId, string FileName, Stream FileContent) : IRequest<Unit>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["lessons"];
}

internal sealed class UploadLessonMaterialCommandHandler(IApplicationDbContext db, IFileStorageService fileStorage) : IRequestHandler<UploadLessonMaterialCommand, Unit>
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".md", ".docx", ".txt" };

    public async Task<Unit> Handle(UploadLessonMaterialCommand request, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(request.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new Application.Common.ValidationException(
                new[] { new ValidationFailure("FileName", $"Only .md, .docx and .txt files are accepted. Got: {extension}") });
        }

        var entity = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);
        Guard.Against.NotFound(request.LessonId, entity);

        if (string.IsNullOrEmpty(entity.AlbumId))
        {
            entity.AlbumId = await fileStorage.CreateAlbumAsync(entity.Title, cancellationToken);
        }

        var existingFiles = await fileStorage.GetAlbumFilesAsync(entity.AlbumId, cancellationToken);
        if (existingFiles.Any(f => string.Equals(f.Name, request.FileName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Application.Common.ValidationException(
                new[] { new ValidationFailure("FileName", $"A file named '{request.FileName}' already exists for this lesson.") });
        }

        var storageKey = await fileStorage.UploadAsync(request.FileName, request.FileContent, cancellationToken);
        await fileStorage.AddFileToAlbumAsync(storageKey, entity.AlbumId, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
