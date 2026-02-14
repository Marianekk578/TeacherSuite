using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<AgeGroup> AgeGroups { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Course> Courses { get; }
    DbSet<Group> Groups { get; }
    DbSet<GroupCourse> GroupCourses { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
