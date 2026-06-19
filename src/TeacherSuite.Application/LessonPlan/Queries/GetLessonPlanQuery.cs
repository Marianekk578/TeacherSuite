using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.LessonPlan.Dtos;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.LessonPlan.Queries;

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record GetLessonPlanQuery(DateTimeOffset? From, DateTimeOffset? To) : IRequest<List<ScheduledLessonDto>>;

internal sealed class GetLessonPlanQueryHandler(
    IApplicationDbContext db,
    IMapper mapper,
    ICurrentUserService currentUser) : IRequestHandler<GetLessonPlanQuery, List<ScheduledLessonDto>>
{
    public async Task<List<ScheduledLessonDto>> Handle(GetLessonPlanQuery request, CancellationToken cancellationToken)
    {
        var query = db.ScheduledLessons.AsQueryable();

        // Teacher-only users see only their own groups
        bool isTeacherOnly = currentUser.IsInRole(AppRoles.Teacher)
            && !currentUser.IsInRole(AppRoles.Admin)
            && !currentUser.IsInRole(AppRoles.Supervisor);

        if (isTeacherOnly)
        {
            var teacher = await db.Teachers
                .FirstOrDefaultAsync(t => t.Email == currentUser.Email, cancellationToken);

            if (teacher is null)
                return [];

            query = query.Where(sl => sl.Group != null && sl.Group.TeacherId == teacher.Id);
        }

        if (request.From.HasValue)
            query = query.Where(sl => sl.ScheduledEnd >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(sl => sl.ScheduledStart <= request.To.Value);

        return await query
            .OrderBy(sl => sl.ScheduledStart)
            .ProjectTo<ScheduledLessonDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
