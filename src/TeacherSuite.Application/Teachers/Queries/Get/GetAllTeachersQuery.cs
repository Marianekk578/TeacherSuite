using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Teachers.Dtos;

namespace TeacherSuite.Application.Teachers.Queries.Get;

public record GetAllTeachersQuery : IRequest<PagedResult<TeacherDto>>
{
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
}

public class GetAllTeachersQueryHandler : IRequestHandler<GetAllTeachersQuery, PagedResult<TeacherDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetAllTeachersQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<TeacherDto>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Teachers
            .Include(t => t.TeacherProgrammingLanguages)
                .ThenInclude(tpl => tpl.ProgrammingLanguage)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(t =>
                (t.FirstName != null && t.FirstName.ToLower().Contains(search)) ||
                (t.LastName != null && t.LastName.ToLower().Contains(search)) ||
                t.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var items = await query
            .OrderBy(t => t.LastName)
            .ThenBy(t => t.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedResult<TeacherDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}