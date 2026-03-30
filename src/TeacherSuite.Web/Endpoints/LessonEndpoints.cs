using MediatR;
using TeacherSuite.Application.Lessons.Commands.Create;
using TeacherSuite.Application.Lessons.Commands.Update;
using TeacherSuite.Application.Lessons.Queries;

namespace TeacherSuite.Web.Endpoints;

public static class LessonEndpoints
{
    public static void MapLessonEndpoints(this WebApplication app)
    {
        app.MapGet("/Lessons", async (Lessons endpoints, ISender sender, [AsParameters] GetLessonsByCourseIdQuery query) =>
            await endpoints.GetLessonsByCourseId(sender, query));
        app.MapGet("/Lessons/{id:int}", async (Lessons endpoints, ISender sender, int id) =>
            await endpoints.GetLessonById(sender, id));
        app.MapPost("/Lessons", async (Lessons endpoints, ISender sender, CreateLessonCommand command) =>
            await endpoints.CreateLesson(sender, command));
        app.MapPut("/Lessons/{id:int}", async (Lessons endpoints, ISender sender, int id, UpdateLessonCommand command) =>
            await endpoints.UpdateLesson(sender, id, command));
        app.MapDelete("/Lessons/{id:int}", async (Lessons endpoints, ISender sender, int id) =>
            await endpoints.DeleteLesson(sender, id));
        app.MapPost("/Lessons/{id:int}/material", async (Lessons endpoints, ISender sender, int id, IFormFile file) =>
            await endpoints.UploadMaterial(sender, id, file)).DisableAntiforgery();
        app.MapGet("/Lessons/{id:int}/material/download", async (Lessons endpoints, ISender sender, int id, string fileUuid) =>
            await endpoints.DownloadMaterial(sender, id, fileUuid));
        app.MapGet("/Lessons/{id:int}/files", async (Lessons endpoints, ISender sender, int id) =>
            await endpoints.GetFiles(sender, id));
        app.MapGet("/Lessons/{id:int}/suggestions", async (Lessons endpoints, ISender sender, int id) =>
            await endpoints.GetSuggestions(sender, id));
        app.MapPost("/Lessons/{id:int}/suggestions", async (Lessons endpoints, ISender sender, int id, CreateSuggestionRequest request) =>
            await endpoints.CreateSuggestion(sender, id, request));
        app.MapDelete("/Lessons/suggestions/{id:guid}", async (Lessons endpoints, ISender sender, Guid id) =>
            await endpoints.DeleteSuggestion(sender, id));
        app.MapPost("/Lessons/suggestions/{id:guid}/vote", async (Lessons endpoints, ISender sender, Guid id, VoteSuggestionRequest request) =>
            await endpoints.VoteSuggestion(sender, id, request));
        app.MapGet("/Lessons/{id:int}/attendances", async (Lessons endpoints, ISender sender, int id) =>
            await endpoints.GetAttendances(sender, id));
        app.MapPost("/Lessons/{id:int}/attendances", async (Lessons endpoints, ISender sender, int id, RecordAttendanceRequest request) =>
            await endpoints.RecordAttendance(sender, id, request));
        app.MapPost("/Lessons/{id:int}/reorder", async (Lessons endpoints, ISender sender, int id, ReorderLessonRequest request) =>
            await endpoints.ReorderLesson(sender, id, request));
        app.MapGet("/Lessons/course/{courseId:int}/groups", async (Lessons endpoints, ISender sender, int courseId) =>
            await endpoints.GetCourseGroups(sender, courseId));
    }
}
