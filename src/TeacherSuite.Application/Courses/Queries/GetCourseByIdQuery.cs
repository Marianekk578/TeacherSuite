using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Courses.Dtos;

namespace TeacherSuite.Application.Courses.Queries;

public record GetCourseByIdQuery(int Id) : IRequest<CourseDto?>;

public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCourseByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CourseDto?> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
    {
        return await _context.Courses
            .Where(c => c.Id == request.Id)
            .ProjectTo<CourseDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
