using MediatR;
using TeacherSuite.Application.ProgrammingLanguages.Commands.Create;
using TeacherSuite.Application.ProgrammingLanguages.Commands.Update;
using TeacherSuite.Application.ProgrammingLanguages.Queries;

namespace TeacherSuite.Web.Endpoints;

public static class ProgrammingLanguageEndpoints
{
    public static void MapProgrammingLanguageEndpoints(this WebApplication app)
    {
        app.MapGet("/ProgrammingLanguages", async (ProgrammingLanguages endpoints, ISender sender, [AsParameters] GetAllProgrammingLanguagesQuery query) =>
            await endpoints.GetAllProgrammingLanguages(sender, query));

        app.MapGet("/ProgrammingLanguages/{id:int}", async (ProgrammingLanguages endpoints, ISender sender, int id) =>
            await endpoints.GetProgrammingLanguageById(sender, id));

        app.MapPost("/ProgrammingLanguages", async (ProgrammingLanguages endpoints, ISender sender, CreateProgrammingLanguageCommand command) =>
            await endpoints.CreateProgrammingLanguage(sender, command));

        app.MapPut("/ProgrammingLanguages/{id:int}", async (ProgrammingLanguages endpoints, ISender sender, int id, UpdateProgrammingLanguageCommand command) =>
            await endpoints.UpdateProgrammingLanguage(sender, id, command));

        app.MapDelete("/ProgrammingLanguages/{id:int}", async (ProgrammingLanguages endpoints, ISender sender, int id) =>
            await endpoints.DeleteProgrammingLanguage(sender, id));
    }
}
