namespace TeacherSuite.Application.ProgrammingLanguages.Commands;

public record DeleteProgrammingLanguageCommand(int Id) : IRequest<bool>;
