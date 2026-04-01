using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Lessons.Commands.VoteSuggestion;

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record VoteLessonSuggestionCommand(Guid SuggestionId, VoteType VoteType) : IRequest<Unit>;

internal sealed class VoteLessonSuggestionCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IRequestHandler<VoteLessonSuggestionCommand, Unit>
{
    public async Task<Unit> Handle(VoteLessonSuggestionCommand request, CancellationToken cancellationToken)
    {
        var suggestion = await db.LessonSuggestions.FindAsync(new object[] { request.SuggestionId }, cancellationToken);

        Guard.Against.NotFound(request.SuggestionId, suggestion);

        var teacher = await db.Teachers
            .FirstOrDefaultAsync(t => t.Email == currentUser.Email, cancellationToken);

        Guard.Against.NotFound(currentUser.Email ?? "unknown", teacher);

        var existingVote = await db.SuggestionVotes
            .FirstOrDefaultAsync(v => v.SuggestionId == request.SuggestionId && v.TeacherId == teacher.Id, cancellationToken);

        if (existingVote is not null)
        {
            if (existingVote.Vote == request.VoteType)
            {
                db.SuggestionVotes.Remove(existingVote);
            }
            else
            {
                existingVote.Vote = request.VoteType;
            }
        }
        else
        {
            db.SuggestionVotes.Add(new SuggestionVote
            {
                SuggestionId = request.SuggestionId,
                TeacherId = teacher.Id,
                Vote = request.VoteType
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
