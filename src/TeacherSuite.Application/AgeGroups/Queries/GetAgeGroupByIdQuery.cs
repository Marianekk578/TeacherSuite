using TeacherSuite.Application.AgeGroups.Dtos;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Application.AgeGroups.Queries;

public record GetAgeGroupByIdQuery(int id) : IRequest<AgeGroupDto?>;

internal sealed class GetAgeGroupByIdQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAgeGroupByIdQuery, AgeGroupDto?>
{
    public async Task<AgeGroupDto?> Handle(GetAgeGroupByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.AgeGroups
            .Where(a => a.Id == request.id)
            .ProjectTo<AgeGroupDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}