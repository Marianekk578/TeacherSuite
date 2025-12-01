using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.ProgrammingLanguages.Commands;
using TeacherSuite.Application.ProgrammingLanguages.Dtos;
using TeacherSuite.Application.ProgrammingLanguages.Queries;

namespace TeacherSuite.Web.Endpoints;

public class ProgrammingLanguages
{
    public async Task<Ok<List<ProgrammingLanguageDto>>> GetProgrammingLanguages(ISender sender, [AsParameters] GetProgrammingLanguagesQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Results<Ok<ProgrammingLanguageDto>, NotFound>> GetProgrammingLanguageById(ISender sender, int id)
    {
        var result = await sender.Send(new GetProgrammingLanguageByIdQuery(id));
        return result != null ? TypedResults.Ok(result) : TypedResults.NotFound();
    }

    public async Task<Created<int>> CreateProgrammingLanguage(ISender sender, CreateProgrammingLanguageCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(ProgrammingLanguages)}/{id}", id);
    }

    public async Task<Results<NoContent, NotFound>> UpdateProgrammingLanguage(ISender sender, int id, UpdateProgrammingLanguageCommand command)
    {
        if (id != command.Id)
        {
            return TypedResults.NotFound();
        }

        var result = await sender.Send(command);
        return result ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    public async Task<Results<NoContent, NotFound>> DeleteProgrammingLanguage(ISender sender, int id)
    {
        var result = await sender.Send(new DeleteProgrammingLanguageCommand(id));
        return result ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
