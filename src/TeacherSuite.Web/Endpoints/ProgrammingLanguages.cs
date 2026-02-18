using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.ProgrammingLanguages.Commands.Create;
using TeacherSuite.Application.ProgrammingLanguages.Commands.Delete;
using TeacherSuite.Application.ProgrammingLanguages.Commands.Update;
using TeacherSuite.Application.ProgrammingLanguages.Dtos;
using TeacherSuite.Application.ProgrammingLanguages.Queries;

namespace TeacherSuite.Web.Endpoints;

public class ProgrammingLanguages
{
    public async Task<Created<int>> CreateProgrammingLanguage(ISender sender, CreateProgrammingLanguageCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(ProgrammingLanguages)}/{id}", id);
    }

    public async Task<Ok<List<ProgrammingLanguageDto>>> GetAllProgrammingLanguages(ISender sender, GetAllProgrammingLanguagesQuery query)
    {
        var programmingLanguages = await sender.Send(query);
        return TypedResults.Ok(programmingLanguages);
    }

    public async Task<Results<Ok<ProgrammingLanguageDto>, NotFound>> GetProgrammingLanguageById(ISender sender, int id)
    {
        var programmingLanguage = await sender.Send(new GetProgrammingLanguageByIdQuery(id));
        return programmingLanguage is null ? TypedResults.NotFound() : TypedResults.Ok(programmingLanguage);
    }

    public async Task<NoContent> UpdateProgrammingLanguage(ISender sender, int id, UpdateProgrammingLanguageCommand command)
    {
        var commandWithId = command with { Id = id };
        await sender.Send(commandWithId);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteProgrammingLanguage(ISender sender, int id)
    {
        await sender.Send(new DeleteProgrammingLanguageCommand(id));
        return TypedResults.NoContent();
    }
}
