using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Dtos;

namespace TeacherSuite.Application.Teachers.Queries.Get;

public record GetAllTeachersQuery : IRequest<List<TeacherDto>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.AllTeachers;
}

public class GetAllTeachersQueryHandler : IRequestHandler<GetAllTeachersQuery, List<TeacherDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetAllTeachersQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<TeacherDto>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        return await _db.Teachers
            .Include(t => t.TeacherProgrammingLanguages)
                .ThenInclude(tpl => tpl.ProgrammingLanguage)
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}