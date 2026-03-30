using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Lessons.Queries;

public record LessonMaterialResult(string FileName, Stream Content);

public record DownloadLessonMaterialQuery(int LessonId, string FileUuid) : IRequest<LessonMaterialResult>;

internal sealed class DownloadLessonMaterialQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage) : IRequestHandler<DownloadLessonMaterialQuery, LessonMaterialResult>
{
    public async Task<LessonMaterialResult> Handle(DownloadLessonMaterialQuery request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);
        Guard.Against.NotFound(request.LessonId, lesson);
        Guard.Against.NullOrEmpty(lesson.AlbumId, nameof(lesson.AlbumId),
            $"Lesson {request.LessonId} has no album.");

        var files = await fileStorage.GetAlbumFilesAsync(lesson.AlbumId, cancellationToken);
        var file = files.FirstOrDefault(f => f.Uuid == request.FileUuid);

        if (file == null)
        {
            throw new NotFoundException(nameof(file), request.FileUuid);
        }

        var content = await fileStorage.DownloadAsync(request.FileUuid, cancellationToken);

        return new LessonMaterialResult(file.Name, content);
    }
}
