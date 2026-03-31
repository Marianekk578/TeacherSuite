using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Lessons.Dtos;

public class LessonAttendanceDto
{
    public Guid Id { get; init; }
    public int LessonId { get; init; }
    public Guid GroupId { get; init; }
    public string? GroupName { get; init; }
    public DateTimeOffset AttendedAt { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<LessonAttendance, LessonAttendanceDto>()
                .ForMember(dest => dest.GroupName,
                    opt => opt.MapFrom(src => src.Group != null ? src.Group.Name : null));
        }
    }
}
