using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Groups.Dtos;

namespace TeacherSuite.Application.Groups.Queries;

public record GetAllGroupsQuery(string? CourseName = null) : IRequest<List<GroupDto>>;

internal sealed class GetAllGroupsQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAllGroupsQuery, List<GroupDto>>
{
    public async Task<List<GroupDto>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Group> query = db.Groups;

        if (!string.IsNullOrWhiteSpace(request.CourseName))
        {
            query = query.Where(g => g.GroupCourses
                .Any(gc => gc.Course != null &&
                           gc.Course.Name != null &&
                           gc.Course.Name.Equals(request.CourseName, StringComparison.OrdinalIgnoreCase)));
        }

        return await query
            .ProjectTo<GroupDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
