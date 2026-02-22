using TeacherSuite.Application.AgeGroups.Dtos;
using TeacherSuite.Application.Courses.Dtos;
using TeacherSuite.Application.Teachers.Dtos;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Groups.Dtos;

public class GroupCourseDto
{
    public int CourseId { get; init; }
    public string? CourseName { get; init; }
    public CourseAssignmentStatus Status { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<GroupCourse, GroupCourseDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course!.Name));
        }
    }
}

public class GroupDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public Guid TeacherId { get; init; }
    public int AgeGroupID { get; init; }
    public TeacherDto? Teacher { get; init; }
    public AgeGroupDto? AgeGroup { get; init; }
    public List<GroupCourseDto> Courses { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Group, GroupDto>()
                .ForMember(dest => dest.Courses, opt => opt.MapFrom(src => src.GroupCourses));
        }
    }
}
