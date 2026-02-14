using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.ProgrammingLanguages.Dtos;

namespace TeacherSuite.Application.ProgrammingLanguages.Queries;

public record GetAllProgrammingLanguagesQuery : IRequest<List<ProgrammingLanguageDto>>;

public class GetAllProgrammingLanguagesQueryHandler : IRequestHandler<GetAllProgrammingLanguagesQuery, List<ProgrammingLanguageDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetAllProgrammingLanguagesQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<ProgrammingLanguageDto>> Handle(GetAllProgrammingLanguagesQuery request, CancellationToken cancellationToken)
    {
        return await _db.ProgrammingLanguages
            .ProjectTo<ProgrammingLanguageDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
