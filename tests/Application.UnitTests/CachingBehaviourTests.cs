using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Behaviours;
using TeacherSuite.Application.Common.Interfaces;

namespace Application.UnitTests;

public class CachingBehaviourTests
{
    private readonly MemoryCache _cache;
    private readonly Mock<ILogger<CachingBehaviour<TestCacheableQuery, string>>> _logger;
    private readonly CachingBehaviour<TestCacheableQuery, string> _sut;

    public CachingBehaviourTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });
        _logger = new Mock<ILogger<CachingBehaviour<TestCacheableQuery, string>>>();
        _sut = new CachingBehaviour<TestCacheableQuery, string>(_cache, _logger.Object);
    }

    [Fact]
    public async Task Handle_CacheableQuery_CachesMissResult()
    {
        // Arrange
        var query = new TestCacheableQuery();
        var expected = "fetched from source";
        var callCount = 0;

        RequestHandlerDelegate<string> next = (_) =>
        {
            callCount++;
            return Task.FromResult(expected);
        };

        // Act
        var result = await _sut.Handle(query, next, CancellationToken.None);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal(1, callCount);
        Assert.True(_cache.TryGetValue(query.CacheKey, out string? cached));
        Assert.Equal(expected, cached);
    }

    [Fact]
    public async Task Handle_CacheableQuery_ReturnsCachedResultOnSecondCall()
    {
        // Arrange
        var query = new TestCacheableQuery();
        var callCount = 0;

        RequestHandlerDelegate<string> next = (_) =>
        {
            callCount++;
            return Task.FromResult("fetched from source");
        };

        // Act
        await _sut.Handle(query, next, CancellationToken.None);
        var result = await _sut.Handle(query, next, CancellationToken.None);

        // Assert
        Assert.Equal("fetched from source", result);
        Assert.Equal(1, callCount); // handler called only once
    }

    [Fact]
    public async Task Handle_NonCacheableQuery_PassesThroughWithoutCaching()
    {
        // Arrange
        var nonCacheableSut = new CachingBehaviour<TestNonCacheableQuery, string>(
            _cache, new Mock<ILogger<CachingBehaviour<TestNonCacheableQuery, string>>>().Object);
        var query = new TestNonCacheableQuery();
        var callCount = 0;

        RequestHandlerDelegate<string> next = (_) =>
        {
            callCount++;
            return Task.FromResult("result");
        };

        // Act
        var result1 = await nonCacheableSut.Handle(query, next, CancellationToken.None);
        var result2 = await nonCacheableSut.Handle(query, next, CancellationToken.None);

        // Assert
        Assert.Equal("result", result1);
        Assert.Equal("result", result2);
        Assert.Equal(2, callCount); // handler called each time
    }

    [Fact]
    public async Task Handle_CacheableQuery_UsesSizeFromQuery()
    {
        // Arrange
        var smallCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1 });
        var sut = new CachingBehaviour<TestCacheableQuery, string>(
            smallCache, _logger.Object);
        var query = new TestCacheableQuery();

        // Act
        await sut.Handle(query, (_) => Task.FromResult("first"), CancellationToken.None);

        // The first entry should be cached (size 1, limit 1)
        Assert.True(smallCache.TryGetValue(query.CacheKey, out _));
    }

    // Test record types
    public record TestCacheableQuery : IRequest<string>, ICacheableQuery
    {
        public string CacheKey => "test-cacheable-key";
    }

    public record TestNonCacheableQuery : IRequest<string>;
}
