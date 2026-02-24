using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.Groups.Commands.AssignCourse;
using TeacherSuite.Application.Groups.Commands.Create;
using TeacherSuite.Application.Groups.Commands.Delete;
using TeacherSuite.Application.Groups.Commands.Update;
using TeacherSuite.Application.Groups.Dtos;
using TeacherSuite.Application.Groups.Queries;

namespace TeacherSuite.Web.Endpoints;

public class Groups
{
    public async Task<Created<Guid>> CreateGroup(ISender sender, CreateGroupCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Groups)}/{id}", id);
    }

    public async Task<Ok<List<GroupDto>>> GetAllGroups(ISender sender, GetAllGroupsQuery query)
    {
        var groups = await sender.Send(query);
        return TypedResults.Ok(groups);
    }

    public async Task<Results<Ok<GroupDto>, NotFound>> GetGroupById(ISender sender, Guid id)
    {
        var group = await sender.Send(new GetGroupByIdQuery(id));
        return group is null ? TypedResults.NotFound() : TypedResults.Ok(group);
    }

    public async Task<NoContent> UpdateGroup(ISender sender, Guid id, UpdateGroupCommand command)
    {
        var commandWithId = command with { Id = id };
        await sender.Send(commandWithId);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteGroup(ISender sender, Guid id)
    {
        await sender.Send(new DeleteGroupCommand(id));
        return TypedResults.NoContent();
    }

    public async Task<NoContent> AssignCourse(ISender sender, Guid groupId, int courseId, AssignCourseToGroupCommand command)
    {
        var commandWithIds = command with { GroupId = groupId, CourseId = courseId };
        await sender.Send(commandWithIds);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> UnassignCourse(ISender sender, Guid groupId, int courseId)
    {
        await sender.Send(new UnassignCourseFromGroupCommand(groupId, courseId));
        return TypedResults.NoContent();
    }

    public async Task<NoContent> UpdateCourseStatus(ISender sender, Guid groupId, int courseId, UpdateGroupCourseStatusCommand command)
    {
        var commandWithIds = command with { GroupId = groupId, CourseId = courseId };
        await sender.Send(commandWithIds);
        return TypedResults.NoContent();
    }
}
