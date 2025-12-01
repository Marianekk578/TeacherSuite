using TeacherSuite.Application.AgeGroups.Common.Interfaces;
using TeacherSuite.Application.ProgrammingLanguages.Dtos;

namespace TeacherSuite.Application.ProgrammingLanguages.Queries;

public record GetProgrammingLanguagesQuery : IRequest<List<ProgrammingLanguageDto>>;

public class GetProgrammingLanguagesQueryHandler : IRequestHandler<GetProgrammingLanguagesQuery, List<ProgrammingLanguageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetProgrammingLanguagesQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<ProgrammingLanguageDto>> Handle(GetProgrammingLanguagesQuery request, CancellationToken cancellationToken)
    {
        return await _db.ProgrammingLanguages
            .ProjectTo<ProgrammingLanguageDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
