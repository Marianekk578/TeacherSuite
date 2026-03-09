using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Courses.Commands.Update;

public record UpdateCourseCommand(int Id, string? Name, string? Description, int AgeGroupID, List<int>? ProgrammingLanguageIds) : IRequest<Unit>;

public class UpdateCourseHandler(IApplicationDbContext context) : IRequestHandler<UpdateCourseCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.Courses
            .Include(c => c.CourseProgrammingLanguages)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.AgeGroupID = request.AgeGroupID;

        var newIds = request.ProgrammingLanguageIds ?? new List<int>();
        var existingIds = entity.CourseProgrammingLanguages.Select(cpl => cpl.ProgrammingLanguageId).ToList();

        var toRemove = entity.CourseProgrammingLanguages.Where(cpl => !newIds.Contains(cpl.ProgrammingLanguageId)).ToList();
        foreach (var cpl in toRemove)
        {
            entity.CourseProgrammingLanguages.Remove(cpl);
        }

        var toAdd = newIds.Where(id => !existingIds.Contains(id)).ToList();
        foreach (var plId in toAdd)
        {
            entity.CourseProgrammingLanguages.Add(new CourseProgrammingLanguage
            {
                CourseId = entity.Id,
                ProgrammingLanguageId = plId
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
