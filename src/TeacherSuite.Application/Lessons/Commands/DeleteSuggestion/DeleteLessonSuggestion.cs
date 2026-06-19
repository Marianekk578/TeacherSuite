using TeacherSuite.Application.Common.Exceptions;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Lessons.Commands.DeleteSuggestion;

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record DeleteLessonSuggestionCommand(Guid Id) : IRequest<Unit>;

internal sealed class DeleteLessonSuggestionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IRequestHandler<DeleteLessonSuggestionCommand, Unit>
{
    public async Task<Unit> Handle(DeleteLessonSuggestionCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.LessonSuggestions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        var isAdminOrSupervisor = currentUser.IsInRole(AppRoles.Admin) || currentUser.IsInRole(AppRoles.Supervisor);

        if (!isAdminOrSupervisor)
        {
            var teacher = await db.Teachers
                .FirstOrDefaultAsync(t => t.Email == currentUser.Email, cancellationToken);

            Guard.Against.NotFound(currentUser.Email ?? "unknown", teacher);

            if (entity.TeacherId != teacher.Id)
            {
                throw new ForbiddenAccessException();
            }
        }

        db.LessonSuggestions.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
