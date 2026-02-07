using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Dtos;

namespace TeacherSuite.Application.Teachers.Queries.Get;

public record GetAllTeachersQuery : IRequest<List<TeacherDto>>;

public class GetAllTeachersQueryHandler : IRequestHandler<GetAllTeachersQuery, List<TeacherDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAllTeachersQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<TeacherDto>> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Teachers
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}