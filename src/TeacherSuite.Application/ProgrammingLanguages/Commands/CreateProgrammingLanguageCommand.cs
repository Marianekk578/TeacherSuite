namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public record CreateProgrammingLanguageCommand(string Name) : IRequest<int>;
