using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Dtos;

namespace TeacherSuite.Application.Teachers.Queries.Get;

public record GetTeacherAssignedToGroupQuery(Guid groupId) : IRequest<TeacherDto?>;

internal sealed class GetTeacherAssignedToGroupQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetTeacherAssignedToGroupQuery, TeacherDto?>
{
    public async Task<TeacherDto?> Handle(GetTeacherAssignedToGroupQuery request, CancellationToken cancellationToken)
    {
        return await db.Teachers
            .Where(t => t.Groups.Any(g => g.Id == request.groupId))
            .ProjectTo<TeacherDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}