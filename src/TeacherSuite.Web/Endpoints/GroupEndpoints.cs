using MediatR;
using TeacherSuite.Application.Groups.Commands.AssignCourse;
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

        app.MapGet("/Groups/by-course-name", async (Groups endpoints, ISender sender, string courseName) =>
            await endpoints.GetGroupsByCourseName(sender, courseName));

        app.MapGet("/Groups/{id:guid}", async (Groups endpoints, ISender sender, Guid id) =>
            await endpoints.GetGroupById(sender, id));

        app.MapPost("/Groups", async (Groups endpoints, ISender sender, CreateGroupCommand command) =>
            await endpoints.CreateGroup(sender, command));

        app.MapPut("/Groups/{id:guid}", async (Groups endpoints, ISender sender, Guid id, UpdateGroupCommand command) =>
            await endpoints.UpdateGroup(sender, id, command));

        app.MapDelete("/Groups/{id:guid}", async (Groups endpoints, ISender sender, Guid id) =>
            await endpoints.DeleteGroup(sender, id));

        app.MapPut("/Groups/{groupId:guid}/courses/{courseId:int}", async (Groups endpoints, ISender sender, Guid groupId, int courseId, AssignCourseToGroupCommand command) =>
            await endpoints.AssignCourse(sender, groupId, courseId, command));

        app.MapDelete("/Groups/{groupId:guid}/courses/{courseId:int}", async (Groups endpoints, ISender sender, Guid groupId, int courseId) =>
            await endpoints.UnassignCourse(sender, groupId, courseId));

        app.MapPatch("/Groups/{groupId:guid}/courses/{courseId:int}/status", async (Groups endpoints, ISender sender, Guid groupId, int courseId, UpdateGroupCourseStatusCommand command) =>
            await endpoints.UpdateCourseStatus(sender, groupId, courseId, command));
    }
}
