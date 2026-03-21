using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Students.Dtos;

public class StudentGroupDto
{
    public Guid GroupId { get; init; }
    public string? GroupName { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<StudentGroup, StudentGroupDto>()
                .ForMember(dest => dest.GroupName, opt => opt.MapFrom(src => src.Group!.Name));
        }
    }
}
