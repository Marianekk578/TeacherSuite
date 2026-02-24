using TeacherSuite.Application.AgeGroups.Dtos;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Courses.Dtos;

public class CourseDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public int AgeGroupID { get; init; }
    public AgeGroupDto? AgeGroup { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Course, CourseDto>();
        }
    }
}
