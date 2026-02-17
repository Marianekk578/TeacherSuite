namespace TeacherSuite.Application.Common.Interfaces;

/// <summary>
/// Marker interface for MediatR requests that should invalidate cached data.
/// </summary>
public interface ICacheInvalidatingCommand
{
    /// <summary>
    /// The cache keys that should be evicted when this command succeeds.
    /// </summary>
    IEnumerable<string> CacheKeysToInvalidate { get; }
}
