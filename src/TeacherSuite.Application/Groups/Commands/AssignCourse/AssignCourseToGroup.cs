using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Entities;
using TeacherSuite.Domain.Enums;

namespace TeacherSuite.Application.Groups.Commands.AssignCourse;

public record AssignCourseToGroupCommand(Guid GroupId, int CourseId, CourseAssignmentStatus Status) : IRequest<Unit>;

public class AssignCourseToGroupHandler(IApplicationDbContext context) : IRequestHandler<AssignCourseToGroupCommand, Unit>
{
    public async Task<Unit> Handle(AssignCourseToGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await context.Groups.FindAsync(new object[] { request.GroupId }, cancellationToken);
        Guard.Against.NotFound(request.GroupId, group);

        var course = await context.Courses.FindAsync(new object[] { request.CourseId }, cancellationToken);
        Guard.Against.NotFound(request.CourseId, course);

        if (group.AgeGroupID != course.AgeGroupID)
        {
            throw new ConflictException("The course's age group does not match the group's age group.");
        }

        var exists = await context.GroupCourses
            .AnyAsync(gc => gc.GroupId == request.GroupId && gc.CourseId == request.CourseId, cancellationToken);

        if (exists)
        {
            throw new ConflictException("This course is already assigned to the group.");
        }

        if (request.Status != CourseAssignmentStatus.Planned && request.Status != CourseAssignmentStatus.Active)
        {
            throw new TeacherSuite.Application.Common.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Status", "Only Planned or Active status is allowed when assigning a course.")
            });
        }

        context.GroupCourses.Add(new GroupCourse
        {
            GroupId = request.GroupId,
            CourseId = request.CourseId,
            StartDate = DateTimeOffset.UtcNow,
            Status = request.Status
        });

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
