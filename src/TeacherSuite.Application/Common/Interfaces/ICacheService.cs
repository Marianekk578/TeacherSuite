namespace TeacherSuite.Application.Common.Interfaces;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? l1Expiration = null,
        TimeSpan? l2Expiration = null,
        IReadOnlyCollection<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default);
}
