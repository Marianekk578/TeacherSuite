using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Groups.Commands.AssignCourse;

public record UpdateGroupCourseStatusCommand(Guid GroupId, int CourseId, CourseAssignmentStatus Status) : IRequest<Unit>;

public class UpdateGroupCourseStatusHandler(IApplicationDbContext context) : IRequestHandler<UpdateGroupCourseStatusCommand, Unit>
{
    private static readonly Dictionary<CourseAssignmentStatus, CourseAssignmentStatus[]> AllowedTransitions = new()
    {
        [CourseAssignmentStatus.Planned] = [CourseAssignmentStatus.Active, CourseAssignmentStatus.Cancelled],
        [CourseAssignmentStatus.Active] = [CourseAssignmentStatus.Completed, CourseAssignmentStatus.Cancelled],
        [CourseAssignmentStatus.Completed] = [],
        [CourseAssignmentStatus.Cancelled] = [],
    };

    public async Task<Unit> Handle(UpdateGroupCourseStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await context.GroupCourses
            .FirstOrDefaultAsync(gc => gc.GroupId == request.GroupId && gc.CourseId == request.CourseId, cancellationToken);

        Guard.Against.NotFound($"{request.GroupId}-{request.CourseId}", entity);

        if (!AllowedTransitions.TryGetValue(entity.Status, out var allowed) || !allowed.Contains(request.Status))
        {
            throw new Application.Common.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Status",
                    $"Cannot transition from {entity.Status} to {request.Status}.")
            });
        }

        entity.Status = request.Status;

        if (request.Status is CourseAssignmentStatus.Completed or CourseAssignmentStatus.Cancelled)
        {
            entity.EndDate = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
