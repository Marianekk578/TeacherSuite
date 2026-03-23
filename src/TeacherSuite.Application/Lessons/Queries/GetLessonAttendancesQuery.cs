using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Lessons.Dtos;

namespace TeacherSuite.Application.Lessons.Queries;

public record GetLessonAttendancesQuery(int LessonId) : IRequest<List<LessonAttendanceDto>>;

internal sealed class GetLessonAttendancesQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetLessonAttendancesQuery, List<LessonAttendanceDto>>
{
    public async Task<List<LessonAttendanceDto>> Handle(GetLessonAttendancesQuery request, CancellationToken cancellationToken)
    {
        return await db.LessonAttendances
            .Include(a => a.Group)
            .Where(a => a.LessonId == request.LessonId)
            .OrderByDescending(a => a.AttendedAt)
            .ProjectTo<LessonAttendanceDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
