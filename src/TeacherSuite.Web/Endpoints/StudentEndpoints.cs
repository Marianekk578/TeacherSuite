using MediatR;
using TeacherSuite.Application.Students.Commands.Create;
using TeacherSuite.Application.Students.Commands.Update;
using TeacherSuite.Application.Students.Queries;

namespace TeacherSuite.Web.Endpoints;

public static class StudentEndpoints
{
    public static void MapStudentEndpoints(this WebApplication app)
    {
        app.MapGet("/Students", async (Students endpoints, ISender sender, [AsParameters] GetAllStudentsQuery query) =>
            await endpoints.GetAllStudents(sender, query));

        app.MapGet("/Students/{id:guid}", async (Students endpoints, ISender sender, Guid id) =>
            await endpoints.GetStudentById(sender, id));

        app.MapPost("/Students", async (Students endpoints, ISender sender, CreateStudentCommand command) =>
            await endpoints.CreateStudent(sender, command));

        app.MapPut("/Students/{id:guid}", async (Students endpoints, ISender sender, Guid id, UpdateStudentCommand command) =>
            await endpoints.UpdateStudent(sender, id, command));

        app.MapDelete("/Students/{id:guid}", async (Students endpoints, ISender sender, Guid id) =>
            await endpoints.DeleteStudent(sender, id));

        app.MapPut("/Students/{studentId:guid}/groups/{groupId:guid}", async (Students endpoints, ISender sender, Guid studentId, Guid groupId) =>
            await endpoints.AssignToGroup(sender, studentId, groupId));

        app.MapDelete("/Students/{studentId:guid}/groups/{groupId:guid}", async (Students endpoints, ISender sender, Guid studentId, Guid groupId) =>
            await endpoints.UnassignFromGroup(sender, studentId, groupId));
    }
}
