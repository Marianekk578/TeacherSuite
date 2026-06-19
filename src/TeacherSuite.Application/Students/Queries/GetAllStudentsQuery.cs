using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Students.Dtos;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Students.Queries;

[Authorize]
public record GetAllStudentsQuery : IRequest<PagedResult<StudentDto>>
{
    public string? Search { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

internal sealed class GetAllStudentsQueryHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser) : IRequestHandler<GetAllStudentsQuery, PagedResult<StudentDto>>
{
    public async Task<PagedResult<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Student> query = db.Students;

        if (currentUser.IsInRole(AppRoles.Teacher)
            && !currentUser.IsInRole(AppRoles.Admin)
            && !currentUser.IsInRole(AppRoles.Supervisor))
        {
            var teacherGroupIds = await db.Groups
                .Where(g => g.Teacher != null && g.Teacher.Email == currentUser.Email)
                .Select(g => g.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(s => s.StudentGroups.Any(sg => teacherGroupIds.Contains(sg.GroupId)));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(s =>
                (s.FirstName != null && s.FirstName.ToLower().Contains(search)) ||
                (s.LastName != null && s.LastName.ToLower().Contains(search)) ||
                (s.FirstName != null && s.LastName != null && (s.FirstName + " " + s.LastName).ToLower().Contains(search)) ||
                s.ContactEmail.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var (page, pageSize) = PaginationDefaults.Normalize(request.Page, request.PageSize);

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
