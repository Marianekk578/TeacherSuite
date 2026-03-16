using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Courses.Commands.Update;

public record UpdateCourseCommand(int Id, string? Name, string? Description, int AgeGroupID, List<int>? ProgrammingLanguageIds) : IRequest<Unit>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["courses"];
}

internal sealed class UpdateCourseCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateCourseCommand, Unit>
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

        var newIds = (request.ProgrammingLanguageIds ?? []).ToHashSet();
        var existingIds = entity.CourseProgrammingLanguages
            .Select(cpl => cpl.ProgrammingLanguageId)
            .ToHashSet();

        foreach (var cpl in entity.CourseProgrammingLanguages
            .Where(cpl => !newIds.Contains(cpl.ProgrammingLanguageId)).ToList())
        {
            entity.CourseProgrammingLanguages.Remove(cpl);
        }

        foreach (var plId in newIds.Except(existingIds))
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
