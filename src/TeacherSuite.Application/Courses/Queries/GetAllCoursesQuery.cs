using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Courses.Dtos;

namespace TeacherSuite.Application.Courses.Queries;

public record GetAllCoursesQuery : IRequest<List<CourseDto>>;

public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, List<CourseDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetAllCoursesQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<CourseDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
    {
        return await _db.Courses
            .ProjectTo<CourseDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
