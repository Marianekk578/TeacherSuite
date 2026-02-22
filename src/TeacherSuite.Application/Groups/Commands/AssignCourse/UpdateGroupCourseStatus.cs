using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Groups.Commands.AssignCourse;

public record UpdateGroupCourseStatusCommand(Guid GroupId, int CourseId, CourseAssignmentStatus Status) : IRequest<Unit>;

public class UpdateGroupCourseStatusHandler(IApplicationDbContext context) : IRequestHandler<UpdateGroupCourseStatusCommand, Unit>
{
    public async Task<Unit> Handle(UpdateGroupCourseStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.GroupCourses
            .FirstOrDefaultAsync(gc => gc.GroupId == request.GroupId && gc.CourseId == request.CourseId, cancellationToken);

        Guard.Against.NotFound($"{request.GroupId}-{request.CourseId}", entity);

        if (request.Status != CourseAssignmentStatus.Cancelled && request.Status != CourseAssignmentStatus.Completed)
        {
            throw new TeacherSuite.Application.Common.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Status", "Only Cancelled or Completed status is allowed when updating a course assignment.")
            });
        }

        entity.Status = request.Status;
        entity.EndDate = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
