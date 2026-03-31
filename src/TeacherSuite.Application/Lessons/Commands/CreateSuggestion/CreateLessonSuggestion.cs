using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Lessons.Commands.CreateSuggestion;

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record CreateLessonSuggestionCommand(
    int LessonId,
    string? Content,
    string? SelectedText,
    int? SelectionStart,
    int? SelectionEnd) : IRequest<Guid>;

internal sealed class CreateLessonSuggestionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IRequestHandler<CreateLessonSuggestionCommand, Guid>
{
    public async Task<Guid> Handle(CreateLessonSuggestionCommand request, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.FindAsync(new object[] { request.LessonId }, cancellationToken);

        Guard.Against.NotFound(request.LessonId, lesson);

        var teacher = await db.Teachers
            .FirstOrDefaultAsync(t => t.Email == currentUser.Email, cancellationToken);

        if (teacher is null)
        {
            teacher = new Teacher
            {
                Id = Guid.NewGuid(),
                FirstName = currentUser.UserName ?? "Unknown",
                LastName = string.Empty,
                Email = currentUser.Email ?? string.Empty,
                PhoneNumber = string.Empty,
                DateOfBirth = DateTimeOffset.UtcNow,
            };
            db.Teachers.Add(teacher);
            await db.SaveChangesAsync(cancellationToken);
        }

        var entity = new LessonSuggestion
        {
            Id = Guid.NewGuid(),
            LessonId = request.LessonId,
            TeacherId = teacher.Id,
            Content = request.Content ?? string.Empty,
            SelectedText = request.SelectedText,
            SelectionStart = request.SelectionStart,
            SelectionEnd = request.SelectionEnd
        };

        db.LessonSuggestions.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
