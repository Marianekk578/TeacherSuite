using MediatR;
using TeacherSuite.Application.Courses.Commands.Create;
using TeacherSuite.Application.Courses.Commands.Update;
using TeacherSuite.Application.Courses.Queries;

namespace TeacherSuite.Web.Endpoints;

public static class CourseEndpoints
{
    public static void MapCourseEndpoints(this WebApplication app)
    {
        app.MapGet("/Courses", async (Courses endpoints, ISender sender, [AsParameters] GetAllCoursesQuery query) =>
            await endpoints.GetAllCourses(sender, query));

        app.MapGet("/Courses/{id:int}", async (Courses endpoints, ISender sender, int id) =>
            await endpoints.GetCourseById(sender, id));

        app.MapPost("/Courses", async (Courses endpoints, ISender sender, CreateCourseCommand command) =>
            await endpoints.CreateCourse(sender, command));

        app.MapPut("/Courses/{id:int}", async (Courses endpoints, ISender sender, int id, UpdateCourseCommand command) =>
            await endpoints.UpdateCourse(sender, id, command));

        app.MapDelete("/Courses/{id:int}", async (Courses endpoints, ISender sender, int id) =>
            await endpoints.DeleteCourse(sender, id));
    }
}
