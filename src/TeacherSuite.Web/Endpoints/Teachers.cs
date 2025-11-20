using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using TeacherSuite.Application.Teachers.Commands.Create;

namespace TeacherSuite.Web.Endpoints;

public class Teachers
{
    public async Task<Created<Guid>> CreateTeachers(ISender sender, CreateTeacherCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/{nameof(Teachers)}/{id}", id);
    }
}
