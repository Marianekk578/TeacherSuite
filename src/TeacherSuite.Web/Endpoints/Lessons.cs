using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.Lessons.Commands.Create;
using TeacherSuite.Application.Lessons.Commands.CreateSuggestion;
using TeacherSuite.Application.Lessons.Commands.Delete;
using TeacherSuite.Application.Lessons.Commands.DeleteSuggestion;
using TeacherSuite.Application.Lessons.Commands.RecordAttendance;
using TeacherSuite.Application.Lessons.Commands.Reorder;
using TeacherSuite.Application.Lessons.Commands.Update;
using TeacherSuite.Application.Lessons.Commands.UploadMaterial;
using TeacherSuite.Application.Lessons.Commands.VoteSuggestion;
using TeacherSuite.Application.Lessons.Dtos;
using TeacherSuite.Application.Lessons.Queries;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Web.Endpoints;

public record CreateSuggestionRequest(string? Content, string? SelectedText, int? SelectionStart, int? SelectionEnd);

public record RecordAttendanceRequest(Guid GroupId, DateTimeOffset AttendedAt);

public record VoteSuggestionRequest(int Vote);

public record ReorderLessonRequest(string Direction);

public class Lessons
{
    public async Task<Ok<List<LessonDto>>> GetLessonsByCourseId(ISender sender, GetLessonsByCourseIdQuery query)
    {
        var lessons = await sender.Send(query);
        return TypedResults.Ok(lessons);
    }

    public async Task<Results<Ok<LessonDetailDto>, NotFound>> GetLessonById(ISender sender, int id)
    {
        var lesson = await sender.Send(new GetLessonByIdQuery(id));
        return lesson is null ? TypedResults.NotFound() : TypedResults.Ok(lesson);
    }

    public async Task<Created<int>> CreateLesson(ISender sender, CreateLessonCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(Lessons)}/{id}", id);
    }

    public async Task<NoContent> UpdateLesson(ISender sender, int id, UpdateLessonCommand command)
    {
        var commandWithId = command with { Id = id };
        await sender.Send(commandWithId);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteLesson(ISender sender, int id)
    {
        await sender.Send(new DeleteLessonCommand(id));
        return TypedResults.NoContent();
    }

    public async Task<NoContent> UploadMaterial(ISender sender, int id, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        await sender.Send(new UploadLessonMaterialCommand(id, file.FileName, stream));
        return TypedResults.NoContent();
    }

    public async Task<IResult> DownloadMaterial(ISender sender, int id)
    {
        var result = await sender.Send(new DownloadLessonMaterialQuery(id));
        return Results.File(result.Content, "application/octet-stream", result.FileName);
    }

    public async Task<Ok<List<LessonSuggestionDto>>> GetSuggestions(ISender sender, int id)
    {
        var suggestions = await sender.Send(new GetLessonSuggestionsQuery(id));
        return TypedResults.Ok(suggestions);
    }

    public async Task<Created<Guid>> CreateSuggestion(ISender sender, int id, CreateSuggestionRequest request)
    {
        var suggestionId = await sender.Send(new CreateLessonSuggestionCommand(
            id, request.Content, request.SelectedText, request.SelectionStart, request.SelectionEnd));
        return TypedResults.Created($"/{nameof(Lessons)}/suggestions/{suggestionId}", suggestionId);
    }

    public async Task<NoContent> DeleteSuggestion(ISender sender, Guid id)
    {
        await sender.Send(new DeleteLessonSuggestionCommand(id));
        return TypedResults.NoContent();
    }

    public async Task<NoContent> VoteSuggestion(ISender sender, Guid id, VoteSuggestionRequest request)
    {
        await sender.Send(new VoteLessonSuggestionCommand(id, (VoteType)request.Vote));
        return TypedResults.NoContent();
    }

    public async Task<Ok<List<LessonAttendanceDto>>> GetAttendances(ISender sender, int id)
    {
        var attendances = await sender.Send(new GetLessonAttendancesQuery(id));
        return TypedResults.Ok(attendances);
    }

    public async Task<Created<Guid>> RecordAttendance(ISender sender, int id, RecordAttendanceRequest request)
    {
        var attendanceId = await sender.Send(new RecordLessonAttendanceCommand(
            id, request.GroupId, request.AttendedAt));
        return TypedResults.Created($"/{nameof(Lessons)}/{id}/attendances/{attendanceId}", attendanceId);
    }

    public async Task<NoContent> ReorderLesson(ISender sender, int id, ReorderLessonRequest request)
    {
        await sender.Send(new ReorderLessonCommand(id, request.Direction));
        return TypedResults.NoContent();
    }
}
