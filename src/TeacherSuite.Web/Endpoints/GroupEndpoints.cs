using MediatR;
using TeacherSuite.Application.Groups.Commands.Create;
using TeacherSuite.Application.Groups.Commands.Update;
using TeacherSuite.Application.Groups.Queries;

namespace TeacherSuite.Web.Endpoints;

public static class GroupEndpoints
{
    public static void MapGroupEndpoints(this WebApplication app)
    {
        app.MapGet("/Groups", async (Groups endpoints, ISender sender, [AsParameters] GetAllGroupsQuery query) =>
            await endpoints.GetAllGroups(sender, query));

        app.MapGet("/Groups/{id:guid}", async (Groups endpoints, ISender sender, Guid id) =>
            await endpoints.GetGroupById(sender, id));

        app.MapPost("/Groups", async (Groups endpoints, ISender sender, CreateGroupCommand command) =>
            await endpoints.CreateGroup(sender, command));

        app.MapPut("/Groups/{id:guid}", async (Groups endpoints, ISender sender, Guid id, UpdateGroupCommand command) =>
            await endpoints.UpdateGroup(sender, id, command));

        app.MapDelete("/Groups/{id:guid}", async (Groups endpoints, ISender sender, Guid id) =>
            await endpoints.DeleteGroup(sender, id));
    }
}
