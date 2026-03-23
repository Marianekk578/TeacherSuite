using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Lessons.Dtos;

public class LessonSuggestionDto
{
    public Guid Id { get; init; }
    public int LessonId { get; init; }
    public Guid TeacherId { get; init; }
    public string? TeacherName { get; init; }
    public string Content { get; init; } = string.Empty;
    public string? SelectedText { get; init; }
    public int? SelectionStart { get; init; }
    public int? SelectionEnd { get; init; }
    public DateTimeOffset Created { get; init; }
    public int UpvoteCount { get; init; }
    public int DownvoteCount { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<LessonSuggestion, LessonSuggestionDto>()
                .ForMember(dest => dest.TeacherName,
                    opt => opt.MapFrom(src => src.Teacher != null
                        ? src.Teacher.FirstName + " " + src.Teacher.LastName
                        : null))
                .ForMember(dest => dest.UpvoteCount,
                    opt => opt.MapFrom(src => src.Votes.Count(v => v.Vote == VoteType.Upvote)))
                .ForMember(dest => dest.DownvoteCount,
                    opt => opt.MapFrom(src => src.Votes.Count(v => v.Vote == VoteType.Downvote)));
        }
    }
}
