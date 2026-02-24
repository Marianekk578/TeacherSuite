using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Dtos;

namespace TeacherSuite.Application.Teachers.Queries.Get;

public record GetTeacherByIdQuery(Guid Id) : IRequest<TeacherDto?>;

public class GetTeacherByIdQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetTeacherByIdQuery, TeacherDto?>
{
    public async Task<TeacherDto?> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Teachers
            .Where(t => t.Id == request.Id)
            .ProjectTo<TeacherDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}