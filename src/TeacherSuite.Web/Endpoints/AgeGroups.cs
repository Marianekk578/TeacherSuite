using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.AgeGroups.Commands;
using TeacherSuite.Application.AgeGroups.Dtos;
using TeacherSuite.Application.AgeGroups.Queries;

namespace TeacherSuite.Web.Endpoints;

public class AgeGroups
{
    public async Task<Ok<List<AgeGroupDto>>> GetAgeGroups(ISender sender, [AsParameters] GetAgeGroupsQuery query)
    {
        var result = await sender.Send(query);
        return TypedResults.Ok(result);
    }

    public async Task<Ok<AgeGroupDto>> GetAgeGroupById(ISender sender, int id)
    {
        var result = await sender.Send(new GetAgeGroupByIdQuery(id));
        return TypedResults.Ok(result);
    }

    public async Task<Created<int>> CreateAgeGroup(ISender sender, CreateAgeGroupCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(AgeGroups)}/{id}", id);
    }
}
