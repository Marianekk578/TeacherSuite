using Moq;
using TeacherSuite.Application.Common.Behaviours;
using TeacherSuite.Application.Common.Exceptions;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using MediatR;

namespace Application.UnitTests;

public class AuthorizationBehaviourTests
{
    [Fact]
    public async Task Handle_WithNoAuthorizeAttribute_CallsNext()
    {
        // Arrange
        var currentUserService = new Mock<ICurrentUserService>();
        var behaviour = new AuthorizationBehaviour<UnprotectedRequest, string>(currentUserService.Object);
        var called = false;

        RequestHandlerDelegate<string> next = (ct) =>
        {
            called = true;
            return Task.FromResult("ok");
        };

        // Act
        var result = await behaviour.Handle(new UnprotectedRequest(), next, CancellationToken.None);

        // Assert
        Assert.True(called);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_WithAuthorizeAttribute_UnauthenticatedUser_ThrowsUnauthorized()
    {
        // Arrange
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(x => x.IsAuthenticated).Returns(false);
        var behaviour = new AuthorizationBehaviour<ProtectedRequest, string>(currentUserService.Object);

        RequestHandlerDelegate<string> next = (ct) => Task.FromResult("ok");

        // Act & Assert
        await Assert.ThrowsAsync<TeacherSuite.Application.Common.Exceptions.UnauthorizedAccessException>(
            () => behaviour.Handle(new ProtectedRequest(), next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithRoleAuthorize_UserHasRole_CallsNext()
    {
        // Arrange
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserService.Setup(x => x.IsInRole(AppRoles.Admin)).Returns(true);
        var behaviour = new AuthorizationBehaviour<AdminOnlyRequest, string>(currentUserService.Object);
        var called = false;

        RequestHandlerDelegate<string> next = (ct) =>
        {
            called = true;
            return Task.FromResult("ok");
        };

        // Act
        var result = await behaviour.Handle(new AdminOnlyRequest(), next, CancellationToken.None);

        // Assert
        Assert.True(called);
    }

    [Fact]
    public async Task Handle_WithRoleAuthorize_UserMissingRole_ThrowsForbidden()
    {
        // Arrange
        var currentUserService = new Mock<ICurrentUserService>();
        currentUserService.Setup(x => x.IsAuthenticated).Returns(true);
        currentUserService.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(false);
        var behaviour = new AuthorizationBehaviour<AdminOnlyRequest, string>(currentUserService.Object);

        RequestHandlerDelegate<string> next = (ct) => Task.FromResult("ok");

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => behaviour.Handle(new AdminOnlyRequest(), next, CancellationToken.None));
    }

    // Test request types
    public class UnprotectedRequest : IRequest<string> { }

    [Authorize]
    public class ProtectedRequest : IRequest<string> { }

    [Authorize(Roles = AppRoles.Admin)]
    public class AdminOnlyRequest : IRequest<string> { }
}
