using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Dtos;

namespace TeacherSuite.Application.Teachers.Queries.Get;

public record GetTeacherAssignedToGroupQuery(Guid groupId) : IRequest<TeacherDto?>;

public class GetTeacherAssignedToGroupQueryHandler : IRequestHandler<GetTeacherAssignedToGroupQuery, TeacherDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetTeacherAssignedToGroupQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TeacherDto?> Handle(GetTeacherAssignedToGroupQuery request, CancellationToken cancellationToken)
    {
        return await _context.Teachers
            .Where(t => t.Groups.Any(g => g.Id == request.groupId))
            .ProjectTo<TeacherDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}