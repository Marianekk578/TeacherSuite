using MediatR;
namespace TeacherSuite.Application.AgeGroups.Commands;

public record CreateAgeGroupCommand(string Name, int MinAge, int MaxAge) : IRequest<int>;