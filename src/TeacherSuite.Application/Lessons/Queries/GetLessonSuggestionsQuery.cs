using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Lessons.Dtos;

namespace TeacherSuite.Application.Lessons.Queries;

public record GetLessonSuggestionsQuery(int LessonId) : IRequest<List<LessonSuggestionDto>>;

internal sealed class GetLessonSuggestionsQueryHandler(IApplicationDbContext db, IMapper mapper) : IRequestHandler<GetLessonSuggestionsQuery, List<LessonSuggestionDto>>
{
    public async Task<List<LessonSuggestionDto>> Handle(GetLessonSuggestionsQuery request, CancellationToken cancellationToken)
    {
        return await db.LessonSuggestions
            .Include(s => s.Teacher)
            .Include(s => s.Votes)
            .Where(s => s.LessonId == request.LessonId)
            .OrderByDescending(s => s.Created)
            .ProjectTo<LessonSuggestionDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
