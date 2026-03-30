using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.LessonPlan.Dtos;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.LessonPlan.Queries;

[Authorize(AppRoles.Policies.AdminSupervisorOrTeacher)]
public record GetScheduledLessonStudentsQuery(Guid ScheduledLessonId) : IRequest<List<StudentAttendanceDto>>;

internal sealed class GetScheduledLessonStudentsQueryHandler(
    IApplicationDbContext db,
    IMapper mapper) : IRequestHandler<GetScheduledLessonStudentsQuery, List<StudentAttendanceDto>>
{
    public async Task<List<StudentAttendanceDto>> Handle(GetScheduledLessonStudentsQuery request, CancellationToken cancellationToken)
    {
        var scheduledLesson = await db.ScheduledLessons
            .FirstOrDefaultAsync(sl => sl.Id == request.ScheduledLessonId, cancellationToken);

        Guard.Against.NotFound(request.ScheduledLessonId, scheduledLesson);

        // Get all students in the group
        var groupStudents = await db.StudentGroups
            .Where(sg => sg.GroupId == scheduledLesson.GroupId)
            .Select(sg => sg.Student!)
            .ToListAsync(cancellationToken);

        // Get existing attendance records
        var attendanceRecords = await db.StudentLessonAttendances
            .Include(sa => sa.Student)
            .Where(sa => sa.ScheduledLessonId == request.ScheduledLessonId)
            .ToListAsync(cancellationToken);

        var attendanceMap = attendanceRecords.ToDictionary(a => a.StudentId);

        // Combine: return all students in the group with their attendance status
        return groupStudents.Select(student =>
        {
            if (attendanceMap.TryGetValue(student.Id, out var attendance))
            {
                return mapper.Map<StudentAttendanceDto>(attendance);
            }

            return new StudentAttendanceDto
            {
                Id = Guid.Empty,
                StudentId = student.Id,
                StudentFirstName = student.FirstName,
                StudentLastName = student.LastName,
                IsPresent = false
            };
        })
        .OrderBy(s => s.StudentLastName)
        .ThenBy(s => s.StudentFirstName)
        .ToList();
    }
}
