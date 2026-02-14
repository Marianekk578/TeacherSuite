using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Groups.Dtos;

namespace TeacherSuite.Application.Groups.Queries;

public record GetGroupByIdQuery(Guid Id) : IRequest<GroupDto?>;

public class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, GroupDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetGroupByIdQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<GroupDto?> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.Groups
            .Include(g => g.Teacher)
            .Include(g => g.AgeGroup)
            .Include(g => g.GroupCourses).ThenInclude(gc => gc.Course)
            .Where(g => g.Id == request.Id)
            .ProjectTo<GroupDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
