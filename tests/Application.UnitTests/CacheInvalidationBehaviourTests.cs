using MediatR;
using Moq;
using TeacherSuite.Application.Common.Behaviours;
using TeacherSuite.Application.Common.Interfaces;

namespace Application.UnitTests;

public class CacheInvalidationBehaviourTests
{
    private record TestInvalidationCommand : IRequest<Unit>, ICacheInvalidationCommand
    {
        public IReadOnlyCollection<string> TagsToInvalidate => ["courses", "agegroups"];
    }

    [Fact]
    public async Task Handle_InvalidatesAllTags_AfterCommandExecution()
    {
        // Arrange
        var cacheService = new Mock<ICacheService>();
        var behaviour = new CacheInvalidationBehaviour<TestInvalidationCommand, Unit>(cacheService.Object);
        var request = new TestInvalidationCommand();
        var handlerCalled = false;
        RequestHandlerDelegate<Unit> next = (_) =>
        {
            handlerCalled = true;
            return Task.FromResult(Unit.Value);
        };

        // Act
        var result = await behaviour.Handle(request, next, CancellationToken.None);

        // Assert
        Assert.True(handlerCalled);
        cacheService.Verify(c => c.InvalidateByTagAsync("courses", It.IsAny<CancellationToken>()), Times.Once);
        cacheService.Verify(c => c.InvalidateByTagAsync("agegroups", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExecutesHandler_BeforeInvalidation()
    {
        // Arrange
        var cacheService = new Mock<ICacheService>();
        var callOrder = new List<string>();

        cacheService
            .Setup(c => c.InvalidateByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("invalidate"))
            .Returns(Task.CompletedTask);

        var behaviour = new CacheInvalidationBehaviour<TestInvalidationCommand, Unit>(cacheService.Object);
        RequestHandlerDelegate<Unit> next = (_) =>
        {
            callOrder.Add("handler");
            return Task.FromResult(Unit.Value);
        };

        // Act
        await behaviour.Handle(new TestInvalidationCommand(), next, CancellationToken.None);

        // Assert
        Assert.Equal("handler", callOrder[0]);
        Assert.True(callOrder.Skip(1).All(c => c == "invalidate"));
    }

    [Fact]
    public async Task Handle_PassesCancellationToken_ToInvalidation()
    {
        // Arrange
        var cacheService = new Mock<ICacheService>();
        var cts = new CancellationTokenSource();

        var behaviour = new CacheInvalidationBehaviour<TestInvalidationCommand, Unit>(cacheService.Object);
        RequestHandlerDelegate<Unit> next = (_) => Task.FromResult(Unit.Value);

        // Act
        await behaviour.Handle(new TestInvalidationCommand(), next, cts.Token);

        // Assert
        cacheService.Verify(c => c.InvalidateByTagAsync(It.IsAny<string>(), cts.Token), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_DoesNotInvalidate_WhenHandlerThrows()
    {
        // Arrange
        var cacheService = new Mock<ICacheService>();
        var behaviour = new CacheInvalidationBehaviour<TestInvalidationCommand, Unit>(cacheService.Object);
        RequestHandlerDelegate<Unit> next = (_) => throw new InvalidOperationException("Handler failed");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behaviour.Handle(new TestInvalidationCommand(), next, CancellationToken.None));

        cacheService.Verify(c => c.InvalidateByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
