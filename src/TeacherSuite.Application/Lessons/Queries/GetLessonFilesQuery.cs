using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Lessons.Queries;

public record LessonFileDto(string Uuid, string Name, long Size);

public record GetLessonFilesQuery(int LessonId) : IRequest<List<LessonFileDto>>;

internal sealed class GetLessonFilesQueryHandler(IApplicationDbContext db, IFileStorageService fileStorage) : IRequestHandler<GetLessonFilesQuery, List<LessonFileDto>>
{
    public async Task<List<LessonFileDto>> Handle(GetLessonFilesQuery request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);
        Guard.Against.NotFound(request.LessonId, lesson);

        if (string.IsNullOrEmpty(lesson.AlbumId))
        {
            return new List<LessonFileDto>();
        }

        var files = await fileStorage.GetAlbumFilesAsync(lesson.AlbumId, cancellationToken);
        return files.Select(f => new LessonFileDto(f.Uuid, f.Name, f.Size)).ToList();
    }
}
