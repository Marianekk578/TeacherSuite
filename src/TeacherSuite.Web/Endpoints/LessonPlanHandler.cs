using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.LessonPlan.Commands.CreateScheduledLesson;
using TeacherSuite.Application.LessonPlan.Commands.ToggleStudentAttendance;
using TeacherSuite.Application.LessonPlan.Dtos;
using TeacherSuite.Application.LessonPlan.Queries;

namespace TeacherSuite.Web.Endpoints;

public record ToggleAttendanceRequest(Guid StudentId, bool IsPresent);

public class LessonPlanHandler
{
    public async Task<Ok<List<ScheduledLessonDto>>> GetLessonPlan(ISender sender, DateTimeOffset? from, DateTimeOffset? to)
    {
        var result = await sender.Send(new GetLessonPlanQuery(from, to));
        return TypedResults.Ok(result);
    }

    public async Task<Created<Guid>> CreateScheduledLesson(ISender sender, CreateScheduledLessonCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/LessonPlan/{id}", id);
    }

    public async Task<Ok<List<StudentAttendanceDto>>> GetScheduledLessonStudents(ISender sender, Guid id)
    {
        var result = await sender.Send(new GetScheduledLessonStudentsQuery(id));
        return TypedResults.Ok(result);
    }

    public async Task<Ok<Guid>> ToggleStudentAttendance(ISender sender, Guid id, ToggleAttendanceRequest request)
    {
        var attendanceId = await sender.Send(new ToggleStudentAttendanceCommand(id, request.StudentId, request.IsPresent));
        return TypedResults.Ok(attendanceId);
    }
}
