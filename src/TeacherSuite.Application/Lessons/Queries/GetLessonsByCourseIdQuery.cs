using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Lessons.Dtos;

namespace TeacherSuite.Application.Lessons.Queries;

public record GetLessonsByCourseIdQuery(int CourseId) : IRequest<List<LessonDto>>, ICacheableQuery
{
    public string CacheKey => $"teachersuite:lessons:course:{CourseId}";
    public IReadOnlyCollection<string>? Tags => ["lessons"];
}

internal sealed class GetLessonsByCourseIdQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetLessonsByCourseIdQuery, List<LessonDto>>
{
    public async Task<List<LessonDto>> Handle(GetLessonsByCourseIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Lessons
            .Include(l => l.LessonRequirementIcons)
                .ThenInclude(lr => lr.RequirementIcon)
            .Where(l => l.CourseId == request.CourseId)
            .OrderBy(l => l.Order)
            .ProjectTo<LessonDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
