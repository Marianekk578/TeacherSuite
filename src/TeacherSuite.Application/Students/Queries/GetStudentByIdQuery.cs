using TeacherSuite.Application.Common.Exceptions;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Application.Students.Dtos;
using TeacherSuite.Domain.Common;

namespace TeacherSuite.Application.Students.Queries;

[Authorize]
public record GetStudentByIdQuery(Guid Id) : IRequest<StudentDetailDto>;

internal sealed class GetStudentByIdQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser) : IRequestHandler<GetStudentByIdQuery, StudentDetailDto>
{
    public async Task<StudentDetailDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await db.Students
            .Include(s => s.StudentGroups)
                .ThenInclude(sg => sg.Group)
                    .ThenInclude(g => g!.AgeGroup)
            .Include(s => s.StudentGroups)
                .ThenInclude(sg => sg.Group)
                    .ThenInclude(g => g!.GroupCourses)
                        .ThenInclude(gc => gc.Course)
                            .ThenInclude(c => c!.CourseProgrammingLanguages)
                                .ThenInclude(cpl => cpl.ProgrammingLanguage)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, student);

        if (currentUser.IsInRole(AppRoles.Teacher)
            && !currentUser.IsInRole(AppRoles.Admin)
            && !currentUser.IsInRole(AppRoles.Supervisor))
        {
            var teacherGroupIds = await db.Groups
                .Where(g => g.Teacher != null && g.Teacher.Email == currentUser.Email)
                .Select(g => g.Id)
                .ToListAsync(cancellationToken);

            if (!student.StudentGroups.Any(sg => teacherGroupIds.Contains(sg.GroupId)))
            {
                throw new ForbiddenAccessException("You can only view students assigned to your groups.");
            }
        }

        var allCourseHistories = student.StudentGroups
            .Where(sg => sg.Group != null)
            .SelectMany(sg => sg.Group!.GroupCourses
                .Where(gc => gc.Course != null)
                .Select(gc => new StudentCourseHistoryDto
                {
                    CourseId = gc.CourseId,
                    CourseName = gc.Course!.Name,
                    GroupName = sg.Group.Name,
                    Status = gc.Status,
                    StartDate = gc.StartDate,
                    EndDate = gc.EndDate
                }))
            .ToList();

        var programmingLanguages = student.StudentGroups
            .Where(sg => sg.Group != null)
            .SelectMany(sg => sg.Group!.GroupCourses
                .Where(gc => gc.Status == Domain.Enums.CourseAssignmentStatus.Completed && gc.Course != null)
                .SelectMany(gc => gc.Course!.CourseProgrammingLanguages
                    .Where(cpl => cpl.ProgrammingLanguage != null)
                    .Select(cpl => cpl.ProgrammingLanguage!)))
            .DistinctBy(pl => pl.Id)
            .Select(pl => new StudentProgrammingLanguageDto
            {
                Id = pl.Id,
                Name = pl.Name,
                Label = pl.Label,
                Color = pl.Color
            })
            .ToList();

        return new StudentDetailDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            DateOfBirth = student.DateOfBirth,
            ContactEmail = student.ContactEmail,
            ContactPhone = student.ContactPhone,
            ParentFirstName = student.ParentFirstName,
            ParentLastName = student.ParentLastName,
            Groups = student.StudentGroups
                .Where(sg => sg.Group != null)
                .Select(sg => new StudentDetailGroupDto
                {
                    GroupId = sg.GroupId,
                    GroupName = sg.Group!.Name,
                    AgeGroup = sg.Group.AgeGroup != null ? new AgeGroups.Dtos.AgeGroupDto
                    {
                        Id = sg.Group.AgeGroup.Id,
                        Name = sg.Group.AgeGroup.Name,
                        Label = sg.Group.AgeGroup.Label,
                        MinAge = sg.Group.AgeGroup.MinAge,
                        MaxAge = sg.Group.AgeGroup.MaxAge
                    } : null
                })
                .ToList(),
            CourseHistory = allCourseHistories,
            ProgrammingLanguages = programmingLanguages
        };
    }
}
