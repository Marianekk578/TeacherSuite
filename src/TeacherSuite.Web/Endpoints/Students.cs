using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.Common.Models;
using TeacherSuite.Application.Students.Commands.AssignGroup;
using TeacherSuite.Application.Students.Commands.Create;
using TeacherSuite.Application.Students.Commands.Delete;
using TeacherSuite.Application.Students.Commands.Update;
using TeacherSuite.Application.Students.Dtos;
using TeacherSuite.Application.Students.Queries;

namespace TeacherSuite.Web.Endpoints;

public class Students
{
    public async Task<Created<Guid>> CreateStudent(ISender sender, CreateStudentCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/{nameof(Students)}/{id}", id);
    }

    public async Task<Ok<PagedResult<StudentDto>>> GetAllStudents(ISender sender, GetAllStudentsQuery query)
    {
        var students = await sender.Send(query);
        return TypedResults.Ok(students);
    }

    public async Task<Results<Ok<StudentDetailDto>, NotFound>> GetStudentById(ISender sender, Guid id)
    {
        var student = await sender.Send(new GetStudentByIdQuery(id));
        return student is null ? TypedResults.NotFound() : TypedResults.Ok(student);
    }

    public async Task<NoContent> UpdateStudent(ISender sender, Guid id, UpdateStudentCommand command)
    {
        var commandWithId = command with { Id = id };
        await sender.Send(commandWithId);
        return TypedResults.NoContent();
    }

    public async Task<NoContent> DeleteStudent(ISender sender, Guid id)
    {
        await sender.Send(new DeleteStudentCommand(id));
        return TypedResults.NoContent();
    }

    public async Task<NoContent> AssignToGroup(ISender sender, Guid studentId, Guid groupId)
    {
        await sender.Send(new AssignStudentToGroupCommand(studentId, groupId));
        return TypedResults.NoContent();
    }

    public async Task<NoContent> UnassignFromGroup(ISender sender, Guid studentId, Guid groupId)
    {
        await sender.Send(new UnassignStudentFromGroupCommand(studentId, groupId));
        return TypedResults.NoContent();
    }
}
