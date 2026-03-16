namespace TeacherSuite.Application.Common.Interfaces;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? L1Expiration => TimeSpan.FromMinutes(2);
    TimeSpan? L2Expiration => TimeSpan.FromMinutes(10);
    IReadOnlyCollection<string>? Tags => null;
}
