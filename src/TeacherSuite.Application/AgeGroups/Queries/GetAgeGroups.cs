using TeacherSuite.Application.AgeGroups.Common.Interfaces;
using TeacherSuite.Application.AgeGroups.Dtos;

namespace TeacherSuite.Application.AgeGroups.Queries;

public record GetAgeGroupsQuery : IRequest<List<AgeGroupDto>>;

public class GetAgeGroupsQueryHandler : IRequestHandler<GetAgeGroupsQuery, List<AgeGroupDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    public GetAgeGroupsQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<AgeGroupDto>> Handle(GetAgeGroupsQuery request, CancellationToken cancellationToken)
    {
        return await _db.AgeGroups
            .ProjectTo<AgeGroupDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}