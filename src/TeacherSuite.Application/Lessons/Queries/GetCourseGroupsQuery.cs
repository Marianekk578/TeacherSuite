using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.Lessons.Queries;

public record CourseGroupDto(Guid Id, string? Name);

public record GetCourseGroupsQuery(int CourseId) : IRequest<List<CourseGroupDto>>;

internal sealed class GetCourseGroupsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetCourseGroupsQuery, List<CourseGroupDto>>
{
    public async Task<List<CourseGroupDto>> Handle(GetCourseGroupsQuery request, CancellationToken cancellationToken)
    {
        return await db.GroupCourses
            .Where(gc => gc.CourseId == request.CourseId)
            .Select(gc => new CourseGroupDto(gc.GroupId, gc.Group != null ? gc.Group.Name : null))
            .ToListAsync(cancellationToken);
    }
}
