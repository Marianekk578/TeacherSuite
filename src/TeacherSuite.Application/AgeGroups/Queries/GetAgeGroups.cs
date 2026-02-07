using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.AgeGroups.Dtos;

namespace TeacherSuite.Application.AgeGroups.Queries;

public record GetAgeGroupsQuery : IRequest<List<AgeGroupDto>>;

public class GetAgeGroupsQueryHandler : IRequestHandler<GetAgeGroupsQuery, List<AgeGroupDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetAgeGroupsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<AgeGroupDto>> Handle(GetAgeGroupsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AgeGroups
            .ProjectTo<AgeGroupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}