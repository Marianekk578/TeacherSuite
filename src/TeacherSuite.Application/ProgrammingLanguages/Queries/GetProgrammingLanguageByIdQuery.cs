using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.ProgrammingLanguages.Dtos;

namespace TeacherSuite.Application.ProgrammingLanguages.Queries;

public record GetProgrammingLanguageByIdQuery(int Id) : IRequest<ProgrammingLanguageDto?>;

internal sealed class GetProgrammingLanguageByIdQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetProgrammingLanguageByIdQuery, ProgrammingLanguageDto?>
{
    public async Task<ProgrammingLanguageDto?> Handle(GetProgrammingLanguageByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.ProgrammingLanguages
            .Where(pl => pl.Id == request.Id)
            .ProjectTo<ProgrammingLanguageDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(cancellationToken);
    }
}
