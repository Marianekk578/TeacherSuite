using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.Teachers.Commands.Create;
using TeacherSuite.Application.Teachers.Commands.Update;
using TeacherSuite.Application.Teachers.Queries.Get;

namespace TeacherSuite.Web.Endpoints;

public class Teachers
{
    public async Task<Created<Guid>> CreateTeachers(ISender sender, CreateTeacherCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Teachers)}/{id}", id);
    }
    public async Task<IResult> GetTeacherAssignedToGroup(ISender sender, GetTeacherAssignedToGroupQuery query)
    {
        var teacher = await sender.Send(query);
        return teacher is null ? Results.NotFound() : Results.Ok(teacher);
    }

    public async Task<IResult> GetAllTeachers(ISender sender, GetAllTeachersQuery query)
    {
        var teachers = await sender.Send(query);
        return Results.Ok(teachers);
    }

    public async Task<IResult> UpdateTeacher(ISender sender, Guid id, UpdateTeacherCommand command)
    {
        var commandWithId = command with { Id = id };

        await sender.Send(commandWithId);

        return TypedResults.NoContent();
    }
}
