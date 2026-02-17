using MediatR;
using TeacherSuite.Application.Teachers.Commands.AssignProgrammingLanguage;
using TeacherSuite.Application.Teachers.Commands.Create;
using TeacherSuite.Application.Teachers.Commands.Delete;
using TeacherSuite.Application.Teachers.Commands.DeleteTestTeachers;
using TeacherSuite.Application.Teachers.Commands.SeedTestTeachers;
using TeacherSuite.Application.Teachers.Commands.Update;
using TeacherSuite.Application.Teachers.Queries.Get;

namespace TeacherSuite.Web.Endpoints;

public static class TeacherEndpoints
{
    public static void MapTeacherEndpoints(this WebApplication app)
    {
        app.MapPost("/Teachers", async (Teachers endpoints, ISender sender, CreateTeacherCommand command) =>
            await endpoints.CreateTeacher(sender, command));

        app.MapGet("/Teachers/assigned", async (Teachers endpoints, ISender sender, [AsParameters] GetTeacherAssignedToGroupQuery query) =>
            await endpoints.GetTeacherAssignedToGroup(sender, query));

        app.MapPut("/Teachers/{id:guid}", async (Teachers endpoints, ISender sender, Guid id, UpdateTeacherCommand command) =>
            await endpoints.UpdateTeacher(sender, id, command));

        app.MapGet("/Teachers", async (Teachers endpoints, ISender sender, [AsParameters] GetAllTeachersQuery query) =>
            await endpoints.GetAllTeachers(sender, query));

        app.MapDelete("/Teachers/{id:guid}", async (Teachers endpoints, ISender sender, Guid id) =>
            await endpoints.DeleteTeacher(sender, id));

        app.MapPost("/Teachers/seed-test", async (Teachers endpoints, ISender sender) =>
            await endpoints.SeedTestTeachers(sender));

        app.MapPost("/Teachers/delete-test", async (Teachers endpoints, ISender sender) =>
            await endpoints.DeleteTestTeachers(sender));

        app.MapPut("/Teachers/{id:guid}/programming-languages/{programmingLanguageId:int}", async (Teachers endpoints, ISender sender, Guid id, int programmingLanguageId) =>
            await endpoints.AssignProgrammingLanguage(sender, id, programmingLanguageId));

        app.MapDelete("/Teachers/{id:guid}/programming-languages/{programmingLanguageId:int}", async (Teachers endpoints, ISender sender, Guid id, int programmingLanguageId) =>
            await endpoints.UnassignProgrammingLanguage(sender, id, programmingLanguageId));
    }
}
