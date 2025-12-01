using TeacherSuite.Application.AgeGroups.Common.Interfaces;
using TeacherSuite.Application.ProgrammingLanguages.Dtos;

namespace TeacherSuite.Application.ProgrammingLanguages.Queries;

public record GetProgrammingLanguageByIdQuery(int Id) : IRequest<ProgrammingLanguageDto?>;

public class GetProgrammingLanguageByIdQueryHandler : IRequestHandler<GetProgrammingLanguageByIdQuery, ProgrammingLanguageDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public GetProgrammingLanguageByIdQueryHandler(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<ProgrammingLanguageDto?> Handle(GetProgrammingLanguageByIdQuery request, CancellationToken cancellationToken)
    {
        return await _db.ProgrammingLanguages
            .Where(pl => pl.Id == request.Id)
            .ProjectTo<ProgrammingLanguageDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
