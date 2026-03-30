using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Lessons.Dtos;

namespace TeacherSuite.Application.Lessons.Queries;

public record GetLessonByIdQuery(int Id) : IRequest<LessonDetailDto?>;

internal sealed class GetLessonByIdQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetLessonByIdQuery, LessonDetailDto?>
{
    public async Task<LessonDetailDto?> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Lessons
            .Include(l => l.Course)
            .Include(l => l.LessonRequirementIcons)
                .ThenInclude(lr => lr.RequirementIcon)
            .Include(l => l.Suggestions)
                .ThenInclude(s => s.Teacher)
            .Include(l => l.Suggestions)
                .ThenInclude(s => s.Votes)
            .Include(l => l.Attendances)
                .ThenInclude(a => a.Group)
            .Where(l => l.Id == request.Id)
            .ProjectTo<LessonDetailDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
