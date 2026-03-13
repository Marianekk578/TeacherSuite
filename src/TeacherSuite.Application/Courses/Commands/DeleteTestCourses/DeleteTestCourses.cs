using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Courses.Commands.Delete;

namespace TeacherSuite.Application.Courses.Commands.DeleteTestCourses;

public record DeleteTestCoursesCommand() : IRequest<int>;

public class DeleteTestCoursesHandler(IApplicationDbContext context, ISender sender)
    : IRequestHandler<DeleteTestCoursesCommand, int>
{
    public async Task<int> Handle(DeleteTestCoursesCommand request, CancellationToken cancellationToken)
    {
        var testCourseIds = await context.Courses
            .Where(c => c.Name != null && c.Name.StartsWith("Test - "))
            .Select(c => c.Id)
            .Take(1000)
            .ToListAsync(cancellationToken);

        foreach (var id in testCourseIds)
        {
            await sender.Send(new DeleteCourseCommand(id), cancellationToken);
        }

        return testCourseIds.Count;
    }
}
