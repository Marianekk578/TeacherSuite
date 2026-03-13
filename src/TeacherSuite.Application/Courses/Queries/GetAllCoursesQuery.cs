using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Courses.Dtos;

namespace TeacherSuite.Application.Courses.Queries;

public record GetAllCoursesQuery : IRequest<PagedResult<CourseDto>>
{
    public string? Search { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

public class GetAllCoursesQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAllCoursesQuery, PagedResult<CourseDto>>
{
    public async Task<PagedResult<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
    {
        var query = db.Courses
            .Include(c => c.AgeGroup)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(c =>
                (c.Name != null && c.Name.ToLower().Contains(search)) ||
                (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page ?? 1);
        var pageSize = Math.Clamp(request.PageSize ?? 12, 1, 100);

        var items = await query
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
