using Microsoft.EntityFrameworkCore.Infrastructure;
using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DatabaseFacade Database { get; }
    DbSet<AgeGroup> AgeGroups { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Course> Courses { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupCourse> GroupCourses { get; }
    DbSet<Student> Students { get; }
    DbSet<StudentGroup> StudentGroups { get; }
    DbSet<ProgrammingLanguage> ProgrammingLanguages { get; }
    DbSet<TeacherProgrammingLanguage> TeacherProgrammingLanguages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
