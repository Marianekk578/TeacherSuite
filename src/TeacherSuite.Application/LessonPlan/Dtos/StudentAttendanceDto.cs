using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.LessonPlan.Dtos;

public class StudentAttendanceDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public string StudentFirstName { get; init; } = string.Empty;
    public string StudentLastName { get; init; } = string.Empty;
    public bool IsPresent { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<StudentLessonAttendance, StudentAttendanceDto>()
                .ForMember(dest => dest.StudentFirstName,
                    opt => opt.MapFrom(src => src.Student != null ? src.Student.FirstName : string.Empty))
                .ForMember(dest => dest.StudentLastName,
                    opt => opt.MapFrom(src => src.Student != null ? src.Student.LastName : string.Empty));
        }
    }
}
