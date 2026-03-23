using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Lessons.Queries;

public record LessonMaterialResult(string FileName, Stream Content);

public record DownloadLessonMaterialQuery(int LessonId) : IRequest<LessonMaterialResult>;

internal sealed class DownloadLessonMaterialQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage) : IRequestHandler<DownloadLessonMaterialQuery, LessonMaterialResult>
{
    public async Task<LessonMaterialResult> Handle(DownloadLessonMaterialQuery request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);

        Guard.Against.NotFound(request.LessonId, lesson);

        Guard.Against.NullOrEmpty(lesson.MaterialStorageKey, nameof(lesson.MaterialStorageKey),
            $"Lesson {request.LessonId} has no material file.");
        Guard.Against.NullOrEmpty(lesson.MaterialFileName, nameof(lesson.MaterialFileName),
            $"Lesson {request.LessonId} has no material file name.");

        var content = await fileStorage.DownloadAsync(lesson.MaterialStorageKey, cancellationToken);

        return new LessonMaterialResult(lesson.MaterialFileName, content);
    }
}
