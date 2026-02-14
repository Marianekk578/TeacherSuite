using TeacherSuite.Application.AgeGroups.Dtos;
using TeacherSuite.Application.Courses.Dtos;
using TeacherSuite.Application.Teachers.Dtos;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Groups.Dtos;

public class GroupDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public Guid TeacherId { get; init; }
    public int AgeGroupID { get; init; }
    public TeacherDto? Teacher { get; init; }
    public AgeGroupDto? AgeGroup { get; init; }
    public CourseDto? Course { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Group, GroupDto>()
                .ForMember(dest => dest.Course, opt => opt.MapFrom(
                    src => src.GroupCourses.Select(gc => gc.Course).FirstOrDefault()));
        }
    }
}
