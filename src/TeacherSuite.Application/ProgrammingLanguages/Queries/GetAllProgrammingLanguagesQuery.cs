using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.ProgrammingLanguages.Dtos;

namespace TeacherSuite.Application.ProgrammingLanguages.Queries;

public record GetAllProgrammingLanguagesQuery : IRequest<List<ProgrammingLanguageDto>>, ICacheableQuery
{
    public string CacheKey => "teachersuite:programming-languages:all";
    public IReadOnlyCollection<string>? Tags => ["programming-languages"];
}

internal sealed class GetAllProgrammingLanguagesQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetAllProgrammingLanguagesQuery, List<ProgrammingLanguageDto>>
{
    public async Task<List<ProgrammingLanguageDto>> Handle(GetAllProgrammingLanguagesQuery request, CancellationToken cancellationToken)
    {
        return await db.ProgrammingLanguages
            .ProjectTo<ProgrammingLanguageDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
