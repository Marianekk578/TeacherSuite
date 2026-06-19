using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.LessonPlan.Dtos;

public class ScheduledLessonDto
{
    public Guid Id { get; init; }
    public int LessonId { get; init; }
    public Guid GroupId { get; init; }
    public string? GroupName { get; init; }
    public string LessonTitle { get; init; } = string.Empty;
    public string? CourseName { get; init; }
    public int CourseId { get; init; }
    public int LessonOrder { get; init; }
    public DateTimeOffset ScheduledStart { get; init; }
    public DateTimeOffset ScheduledEnd { get; init; }
    public bool HasAttendance { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ScheduledLesson, ScheduledLessonDto>()
                .ForMember(dest => dest.GroupName,
                    opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : null))
                .ForMember(dest => dest.LessonTitle,
                    opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Title : string.Empty))
                .ForMember(dest => dest.CourseName,
                    opt => opt.MapFrom(src => src.Lesson != null && src.Lesson.Course != null ? src.Lesson.Course.Name : null))
                .ForMember(dest => dest.CourseId,
                    opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.CourseId : 0))
                .ForMember(dest => dest.LessonOrder,
                    opt => opt.MapFrom(src => src.Lesson != null ? src.Lesson.Order : 0))
                .ForMember(dest => dest.HasAttendance,
                    opt => opt.MapFrom(src => src.StudentAttendances != null && src.StudentAttendances.Count > 0));
        }
    }
}
