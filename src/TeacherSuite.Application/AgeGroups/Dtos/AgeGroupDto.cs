namespace TeacherSuite.Application.AgeGroups.Dtos;

public record AgeGroupDto(int Id, string? Name, int MinAge, int MaxAge);