using MediatR;
using TeacherSuite.Application.LessonPlan.Commands.CreateScheduledLesson;
using TeacherSuite.Application.LessonPlan.Commands.ToggleStudentAttendance;
using TeacherSuite.Application.LessonPlan.Queries;

namespace TeacherSuite.Web.Endpoints;

public static class LessonPlanEndpoints
{
    public static void MapLessonPlanEndpoints(this WebApplication app)
    {
        app.MapGet("/LessonPlan", async (LessonPlanHandler handler, ISender sender,
                DateTimeOffset? from, DateTimeOffset? to) =>
            await handler.GetLessonPlan(sender, from, to));

        app.MapPost("/LessonPlan", async (LessonPlanHandler handler, ISender sender,
                CreateScheduledLessonCommand command) =>
            await handler.CreateScheduledLesson(sender, command));

        app.MapGet("/LessonPlan/{id:guid}/students", async (LessonPlanHandler handler, ISender sender, Guid id) =>
            await handler.GetScheduledLessonStudents(sender, id));

        app.MapPost("/LessonPlan/{id:guid}/attendance", async (LessonPlanHandler handler, ISender sender,
                Guid id, ToggleAttendanceRequest request) =>
            await handler.ToggleStudentAttendance(sender, id, request));
    }
}
