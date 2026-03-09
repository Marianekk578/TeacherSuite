using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Courses.Dtos;

namespace TeacherSuite.Application.Courses.Queries;

public record GetCourseByIdQuery(int Id) : IRequest<CourseDto?>;

public class GetCourseByIdQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetCourseByIdQuery, CourseDto?>
{
    public async Task<CourseDto?> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Courses
            .Include(c => c.AgeGroup)
            .Include(c => c.CourseProgrammingLanguages)
                .ThenInclude(cpl => cpl.ProgrammingLanguage)
            .Where(c => c.Id == request.Id)
            .ProjectTo<CourseDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
