using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TeacherSuite.Application.Common;
using TeacherSuite.Application.Common.Behaviours;
using TeacherSuite.Application.Common.Interfaces;

namespace Application.UnitTests;

public class CacheInvalidationBehaviourTests
{
    private readonly MemoryCache _cache;
    private readonly Mock<ILogger<CacheInvalidationBehaviour<TestInvalidatingCommand, string>>> _logger;
    private readonly CacheInvalidationBehaviour<TestInvalidatingCommand, string> _sut;

    public CacheInvalidationBehaviourTests()
    {
        _cache = new MemoryCache(new MemoryCacheOptions { SizeLimit = 1024 });
        _logger = new Mock<ILogger<CacheInvalidationBehaviour<TestInvalidatingCommand, string>>>();
        _sut = new CacheInvalidationBehaviour<TestInvalidatingCommand, string>(_cache, _logger.Object);
    }

    [Fact]
    public async Task Handle_InvalidatingCommand_RemovesCachedEntry()
    {
        // Arrange - pre-populate cache
        _cache.Set("key-to-invalidate", "cached value", new MemoryCacheEntryOptions { Size = 1 });
        Assert.True(_cache.TryGetValue("key-to-invalidate", out _));

        var command = new TestInvalidatingCommand();

        // Act
        await _sut.Handle(command, (_) => Task.FromResult("command result"), CancellationToken.None);

        // Assert
        Assert.False(_cache.TryGetValue("key-to-invalidate", out _));
    }

    [Fact]
    public async Task Handle_InvalidatingCommand_ExecutesHandlerFirst()
    {
        // Arrange
        var command = new TestInvalidatingCommand();
        var handlerExecuted = false;

        // Act
        var result = await _sut.Handle(command, (_) =>
        {
            handlerExecuted = true;
            return Task.FromResult("result");
        }, CancellationToken.None);

        // Assert
        Assert.True(handlerExecuted);
        Assert.Equal("result", result);
    }

    [Fact]
    public async Task Handle_NonInvalidatingCommand_DoesNotRemoveCachedEntry()
    {
        // Arrange
        _cache.Set("some-key", "cached value", new MemoryCacheEntryOptions { Size = 1 });

        var nonInvalidatingSut = new CacheInvalidationBehaviour<TestNonInvalidatingCommand, string>(
            _cache, new Mock<ILogger<CacheInvalidationBehaviour<TestNonInvalidatingCommand, string>>>().Object);

        var command = new TestNonInvalidatingCommand();

        // Act
        await nonInvalidatingSut.Handle(command, (_) => Task.FromResult("result"), CancellationToken.None);

        // Assert
        Assert.True(_cache.TryGetValue("some-key", out _)); // still cached
    }

    [Fact]
    public async Task Handle_InvalidatingCommand_RemovesMultipleKeys()
    {
        // Arrange
        _cache.Set("key-to-invalidate", "value1", new MemoryCacheEntryOptions { Size = 1 });
        _cache.Set("another-key", "value2", new MemoryCacheEntryOptions { Size = 1 });

        var multiKeySut = new CacheInvalidationBehaviour<TestMultiKeyInvalidatingCommand, string>(
            _cache, new Mock<ILogger<CacheInvalidationBehaviour<TestMultiKeyInvalidatingCommand, string>>>().Object);

        var command = new TestMultiKeyInvalidatingCommand();

        // Act
        await multiKeySut.Handle(command, (_) => Task.FromResult("result"), CancellationToken.None);

        // Assert
        Assert.False(_cache.TryGetValue("key-to-invalidate", out _));
        Assert.False(_cache.TryGetValue("another-key", out _));
    }

    // Test types
    public record TestInvalidatingCommand : IRequest<string>, ICacheInvalidatingCommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["key-to-invalidate"];
    }

    public record TestNonInvalidatingCommand : IRequest<string>;

    public record TestMultiKeyInvalidatingCommand : IRequest<string>, ICacheInvalidatingCommand
    {
        public IEnumerable<string> CacheKeysToInvalidate => ["key-to-invalidate", "another-key"];
    }
}
