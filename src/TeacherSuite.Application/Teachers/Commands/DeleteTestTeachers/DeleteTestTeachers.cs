using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Teachers.Commands.Delete;

namespace TeacherSuite.Application.Teachers.Commands.DeleteTestTeachers;

public record DeleteTestTeachersCommand() : IRequest<int>;

public class DeleteTestTeachersHandler(IApplicationDbContext context, ISender sender)
    : IRequestHandler<DeleteTestTeachersCommand, int>
{
    public async Task<int> Handle(DeleteTestTeachersCommand request, CancellationToken cancellationToken)
    {
        var testTeacherIds = await context.Teachers
            .Where(t => t.LastName == "Testowski")
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var deleted = 0;

        foreach (var id in testTeacherIds)
        {
            await sender.Send(new DeleteTeacherCommand(id), cancellationToken);
            deleted++;
        }

        return deleted;
    }
}
