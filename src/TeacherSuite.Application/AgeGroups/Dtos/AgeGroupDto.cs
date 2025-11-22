using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.AgeGroups.Dtos;

public class AgeGroupDto
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public int MinAge { get; init; }
    public int MaxAge { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<AgeGroup, AgeGroupDto>();
        }
    }
}