using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.Teachers.Commands.AssignProgrammingLanguage;
using TeacherSuite.Application.Teachers.Commands.Create;
using TeacherSuite.Application.Teachers.Commands.Delete;
using TeacherSuite.Application.Teachers.Commands.DeleteTestTeachers;
using TeacherSuite.Application.Teachers.Commands.SeedTestTeachers;
using TeacherSuite.Application.Teachers.Commands.Update;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Teachers.Dtos;
using TeacherSuite.Application.Teachers.Queries.Get;

namespace TeacherSuite.Web.Endpoints;

public class Teachers
{
    public async Task<Created<Guid>> CreateTeacher(ISender sender, CreateTeacherCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Teachers)}/{id}", id);
    }

    public async Task<Results<Ok<TeacherDto>, NotFound>> GetTeacherAssignedToGroup(ISender sender, GetTeacherAssignedToGroupQuery query)
    {
        var teacher = await sender.Send(query);
        return teacher is null ? TypedResults.NotFound() : TypedResults.Ok(teacher);
    }

    public async Task<Ok<PagedResult<TeacherDto>>> GetAllTeachers(ISender sender, GetAllTeachersQuery query)
    {
        var teachers = await sender.Send(query);
        return TypedResults.Ok(teachers);
    }

    public async Task<NoContent> UpdateTeacher(ISender sender, Guid id, UpdateTeacherCommand command)
    {
        var commandWithId = command with { Id = id };

        await sender.Send(commandWithId);

        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteTeacher(ISender sender, Guid id)
    {
        await sender.Send(new DeleteTeacherCommand(id));
        return TypedResults.NoContent();
    }

    public async Task<Ok<int>> SeedTestTeachers(ISender sender)
    {
        var count = await sender.Send(new SeedTestTeachersCommand());
        return TypedResults.Ok(count);
    }

    public async Task<Ok<int>> DeleteTestTeachers(ISender sender)
    {
        var count = await sender.Send(new DeleteTestTeachersCommand());
        return TypedResults.Ok(count);
    }

    public async Task<NoContent> AssignProgrammingLanguage(ISender sender, Guid teacherId, int programmingLanguageId)
    {
        await sender.Send(new AssignProgrammingLanguageCommand(teacherId, programmingLanguageId));
        return TypedResults.NoContent();
    }

    public async Task<NoContent> UnassignProgrammingLanguage(ISender sender, Guid teacherId, int programmingLanguageId)
    {
        await sender.Send(new UnassignProgrammingLanguageCommand(teacherId, programmingLanguageId));
        return TypedResults.NoContent();
    }
}
