using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Courses.Dtos;

namespace TeacherSuite.Application.Courses.Queries;

public record GetAllCoursesQuery : IRequest<List<CourseDto>>;

internal sealed class GetAllCoursesQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAllCoursesQuery, List<CourseDto>>
{
    public async Task<List<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
    {
        return await db.Courses
            .Include(c => c.AgeGroup)
            .ProjectTo<CourseDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
