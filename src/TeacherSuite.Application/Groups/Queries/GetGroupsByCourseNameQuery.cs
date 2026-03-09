using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Groups.Dtos;

namespace TeacherSuite.Application.Groups.Queries;

public record GetGroupsByCourseNameQuery(string CourseName) : IRequest<List<GroupDto>>;

public class GetGroupsByCourseNameQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetGroupsByCourseNameQuery, List<GroupDto>>
{
    public async Task<List<GroupDto>> Handle(GetGroupsByCourseNameQuery request, CancellationToken cancellationToken)
    {
        var courseNameLower = request.CourseName.ToLower();

        return await db.Groups
            .Where(g => g.GroupCourses.Any(gc => gc.Course!.Name!.ToLower() == courseNameLower))
            .ProjectTo<GroupDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
