using MediatR;
using Moq;
using TeacherSuite.Application.Common.Behaviours;
using TeacherSuite.Application.Common.Interfaces;

namespace Application.UnitTests;

public class CachingBehaviourTests
{
    private record TestCacheableQuery : IRequest<string>, ICacheableQuery
    {
        public string CacheKey => "teachersuite:test:all";
        public TimeSpan? L1Expiration => TimeSpan.FromMinutes(1);
        public TimeSpan? L2Expiration => TimeSpan.FromMinutes(5);
        public IReadOnlyCollection<string>? Tags => ["test"];
    }

    [Fact]
    public async Task Handle_CallsCacheServiceWithCorrectParameters()
    {
        // Arrange
        var cacheService = new Mock<ICacheService>();
        var expectedResult = "cached-result";

        cacheService
            .Setup(c => c.GetOrCreateAsync(
                "teachersuite:test:all",
                It.IsAny<Func<CancellationToken, Task<string>>>(),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                It.Is<IReadOnlyCollection<string>>(t => t.Contains("test")),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var behaviour = new CachingBehaviour<TestCacheableQuery, string>(cacheService.Object);
        var request = new TestCacheableQuery();
        RequestHandlerDelegate<string> next = (_) => Task.FromResult("handler-result");

        // Act
        var result = await behaviour.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        cacheService.Verify(c => c.GetOrCreateAsync(
            "teachersuite:test:all",
            It.IsAny<Func<CancellationToken, Task<string>>>(),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            It.Is<IReadOnlyCollection<string>>(t => t.Contains("test")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UsesDefaultExpiration_WhenNotSpecified()
    {
        // Arrange
        var cacheService = new Mock<ICacheService>();

        cacheService
            .Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<string>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("result");

        var behaviour = new CachingBehaviour<TestCacheableQuery, string>(cacheService.Object);
        RequestHandlerDelegate<string> next = (_) => Task.FromResult("result");

        // Act
        await behaviour.Handle(new TestCacheableQuery(), next, CancellationToken.None);

        // Assert
        cacheService.Verify(c => c.GetOrCreateAsync(
            "teachersuite:test:all",
            It.IsAny<Func<CancellationToken, Task<string>>>(),
            TimeSpan.FromMinutes(1),
            TimeSpan.FromMinutes(5),
            It.IsAny<IReadOnlyCollection<string>?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PassesCancellationToken()
    {
        // Arrange
        var cacheService = new Mock<ICacheService>();
        var cts = new CancellationTokenSource();

        cacheService
            .Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<string>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<IReadOnlyCollection<string>?>(),
                cts.Token))
            .ReturnsAsync("result");

        var behaviour = new CachingBehaviour<TestCacheableQuery, string>(cacheService.Object);
        RequestHandlerDelegate<string> next = (_) => Task.FromResult("result");

        // Act
        await behaviour.Handle(new TestCacheableQuery(), next, cts.Token);

        // Assert
        cacheService.Verify(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, Task<string>>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<IReadOnlyCollection<string>?>(),
            cts.Token), Times.Once);
    }
}
