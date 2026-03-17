using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Students.Dtos;

namespace TeacherSuite.Application.Students.Queries;

[Authorize]
public record GetAllStudentsQuery : IRequest<PagedResult<StudentDto>>
{
    public string? Search { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

internal sealed class GetAllStudentsQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAllStudentsQuery, PagedResult<StudentDto>>
{
    public async Task<PagedResult<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Student> query = db.Students;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(s =>
                (s.FirstName != null && s.FirstName.ToLower().Contains(search)) ||
                (s.LastName != null && s.LastName.ToLower().Contains(search)) ||
                s.ContactEmail.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page ?? 1);
        var pageSize = Math.Clamp(request.PageSize ?? 12, 1, 100);

        var items = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<StudentDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<StudentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
