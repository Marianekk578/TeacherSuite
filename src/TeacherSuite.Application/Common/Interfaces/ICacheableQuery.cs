namespace TeacherSuite.Application.Common.Interfaces;

/// <summary>
/// Marker interface for MediatR requests whose results should be cached in memory.
/// Cache keys must be predefined constants—never derived from external user input.
/// </summary>
public interface ICacheableQuery
{
    /// <summary>
    /// A predefined cache key. Must not be derived from user input.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// How long the cached entry should live before it expires.
    /// </summary>
    TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(5);

    /// <summary>
    /// The size of the cache entry relative to other entries (used with SizeLimit).
    /// </summary>
    long Size => 1;
}
