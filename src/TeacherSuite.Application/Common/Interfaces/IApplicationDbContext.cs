using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<AgeGroup> AgeGroups { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Course> Courses { get; }
    DbSet<ProgrammingLanguage> ProgrammingLanguages { get; }
    DbSet<TeacherProgrammingLanguage> TeacherProgrammingLanguages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
