using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Groups.Dtos;

namespace TeacherSuite.Application.Groups.Queries;

public record GetGroupByIdQuery(Guid Id) : IRequest<GroupDto?>;

internal sealed class GetGroupByIdQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetGroupByIdQuery, GroupDto?>
{
    public async Task<GroupDto?> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Groups
            .Where(g => g.Id == request.Id)
            .ProjectTo<GroupDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
