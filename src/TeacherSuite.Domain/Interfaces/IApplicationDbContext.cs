using TeacherSuite.Domain.Entities;

namespace TeacherSuite.Domain.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<AgeGroup> AgeGroups { get; }
    void Add<T>(T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
