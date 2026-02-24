using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Groups.Dtos;

namespace TeacherSuite.Application.Groups.Queries;

public record GetAllGroupsQuery : IRequest<List<GroupDto>>;

public class GetAllGroupsQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAllGroupsQuery, List<GroupDto>>
{
    public async Task<List<GroupDto>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
    {
        return await db.Groups
            .ProjectTo<GroupDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
