using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Dtos;

namespace TeacherSuite.Application.Teachers.Queries.Get;

public record GetTeacherByIdQuery(Guid Id) : IRequest<TeacherDto?>;

public class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, TeacherDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetTeacherByIdQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<TeacherDto?> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.Teachers
            .Where(t => t.Id == request.Id)
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}