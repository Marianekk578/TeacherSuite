using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Courses.Dtos;

namespace TeacherSuite.Application.Courses.Queries;

public record GetAllCoursesQuery : IRequest<PagedResult<CourseDto>>, ICacheableQuery
{
    public string CacheKey => $"teachersuite:courses:page:{Page ?? 1}:size:{PageSize ?? 12}";
    public IReadOnlyCollection<string>? Tags => ["courses"];
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

internal sealed class GetAllCoursesQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAllCoursesQuery, PagedResult<CourseDto>>
{
    public async Task<PagedResult<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await db.Courses.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page ?? 1);
        var pageSize = Math.Clamp(request.PageSize ?? 12, 1, 100);

        var items = await db.Courses
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<CourseDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<CourseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
