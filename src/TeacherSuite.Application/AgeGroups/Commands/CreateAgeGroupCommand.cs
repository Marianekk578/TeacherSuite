using MediatR;
using TeacherSuite.Application.AgeGroups.Dtos;

namespace TeacherSuite.Application.AgeGroups.Commands;

public record CreateAgeGroupCommand(string Name, int MinAge, int MaxAge) : IRequest<AgeGroupDto>;