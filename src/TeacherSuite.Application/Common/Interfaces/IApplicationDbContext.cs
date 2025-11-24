using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Application.AgeGroups.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<AgeGroup> AgeGroups { get; }
    DbSet<Teacher> Teachers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
