using MediatR;
using TeacherSuite.Application.AgeGroups.Commands;
using TeacherSuite.Application.AgeGroups.Queries;

namespace TeacherSuite.Web.Endpoints;

public static class AgeGroupEndpoints
{
    public static void MapAgeGroupEndpoints(this WebApplication app)
    {
        app.MapGet("/AgeGroups", async (AgeGroups endpoints, ISender sender, [AsParameters] GetAgeGroupsQuery query) =>
            await endpoints.GetAgeGroups(sender, query));

        app.MapGet("/AgeGroups/{id:int}", async (AgeGroups endpoints, ISender sender, int id) =>
            await endpoints.GetAgeGroupById(sender, id));

        app.MapPost("/AgeGroups", async (AgeGroups endpoints, ISender sender, CreateAgeGroupCommand command) =>
            await endpoints.CreateAgeGroup(sender, command));
    }
}
