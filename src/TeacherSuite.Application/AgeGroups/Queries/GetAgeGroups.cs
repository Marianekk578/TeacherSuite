using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.AgeGroups.Dtos;

namespace TeacherSuite.Application.AgeGroups.Queries;

public record GetAgeGroupsQuery : IRequest<List<AgeGroupDto>>;

public class GetAgeGroupsQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAgeGroupsQuery, List<AgeGroupDto>>
{
    public async Task<List<AgeGroupDto>> Handle(GetAgeGroupsQuery request, CancellationToken cancellationToken)
    {
        return await db.AgeGroups
            .ProjectTo<AgeGroupDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}