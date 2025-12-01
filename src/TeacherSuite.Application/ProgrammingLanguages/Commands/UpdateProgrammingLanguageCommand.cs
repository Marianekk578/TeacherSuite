namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public record UpdateProgrammingLanguageCommand(int Id, string Name) : IRequest<bool>;
