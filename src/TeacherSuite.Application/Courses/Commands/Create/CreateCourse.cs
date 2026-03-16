using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Events;

namespace TeacherSuite.Application.Courses.Commands.Create;

public record CreateCourseCommand(string? Name, string? Description, int AgeGroupID, List<int>? ProgrammingLanguageIds) : IRequest<int>, ICacheInvalidationCommand
{
    public IReadOnlyCollection<string> TagsToInvalidate => ["courses"];
}

internal sealed class CreateCourseCommandHandler(IApplicationDbContext db, IPublisher publisher) : IRequestHandler<CreateCourseCommand, int>
{
    public async Task<int> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var entity = new Course
        {
            Name = request.Name,
            Description = request.Description,
            AgeGroupID = request.AgeGroupID
        };

        var languagesSet = request.ProgrammingLanguageIds?.ToHashSet();

        if (languagesSet is { Count: > 0 })
        {
            foreach (var plId in languagesSet)
            {
                entity.CourseProgrammingLanguages.Add(new CourseProgrammingLanguage
                {
                    ProgrammingLanguageId = plId
                });
            }
        }

        db.Courses.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new CourseCreatedEvent(entity), cancellationToken);

        return entity.Id;
    }
}
