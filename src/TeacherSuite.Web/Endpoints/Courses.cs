using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Courses.Commands.Create;
using TeacherSuite.Application.Courses.Commands.Delete;
using TeacherSuite.Application.Courses.Commands.Update;
using TeacherSuite.Application.Courses.Dtos;
using TeacherSuite.Application.Courses.Queries;

namespace TeacherSuite.Web.Endpoints;

public class Courses
{
    public async Task<Created<int>> CreateCourse(ISender sender, CreateCourseCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Courses)}/{id}", id);
    }

    public async Task<Ok<PagedResult<CourseDto>>> GetAllCourses(ISender sender, GetAllCoursesQuery query)
    {
        var courses = await sender.Send(query);
        return TypedResults.Ok(courses);
    }

    public async Task<Results<Ok<CourseDto>, NotFound>> GetCourseById(ISender sender, int id)
    {
        var course = await sender.Send(new GetCourseByIdQuery(id));
        return course is null ? TypedResults.NotFound() : TypedResults.Ok(course);
    }

    public async Task<NoContent> UpdateCourse(ISender sender, int id, UpdateCourseCommand command)
    {
        var commandWithId = command with { Id = id };
        await sender.Send(commandWithId);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteCourse(ISender sender, int id)
    {
        await sender.Send(new DeleteCourseCommand(id));
        return TypedResults.NoContent();
    }
}
