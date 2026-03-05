using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using TeacherSuite.Web.Services;

namespace Application.UnitTests;

public class CurrentUserServiceTests
{
    [Fact]
    public void UserId_ReturnsNameIdentifierClaim()
    {
        // Arrange
        var service = CreateServiceWithClaims(new Claim(ClaimTypes.NameIdentifier, "user-123"));

        // Assert
        Assert.Equal("user-123", service.UserId);
    }

    [Fact]
    public void UserName_ReturnsPreferredUsernameClaim()
    {
        // Arrange
        var service = CreateServiceWithClaims(new Claim("preferred_username", "john.doe"));

        // Assert
        Assert.Equal("john.doe", service.UserName);
    }

    [Fact]
    public void Email_ReturnsEmailClaim()
    {
        // Arrange
        var service = CreateServiceWithClaims(new Claim(ClaimTypes.Email, "john@example.com"));

        // Assert
        Assert.Equal("john@example.com", service.Email);
    }

    [Fact]
    public void Roles_ReturnsAllRoleClaims()
    {
        // Arrange
        var service = CreateServiceWithClaims(
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "teacher"));

        // Assert
        Assert.Equal(2, service.Roles.Count);
        Assert.Contains("admin", service.Roles);
        Assert.Contains("teacher", service.Roles);
    }

    [Fact]
    public void IsAuthenticated_WhenAuthenticated_ReturnsTrue()
    {
        // Arrange
        var service = CreateServiceWithClaims(new Claim(ClaimTypes.NameIdentifier, "user-123"));

        // Assert
        Assert.True(service.IsAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_WhenNoContext_ReturnsFalse()
    {
        // Arrange
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        var service = new CurrentUserService(httpContextAccessor.Object);

        // Assert
        Assert.False(service.IsAuthenticated);
    }

    [Fact]
    public void IsInRole_WithMatchingRole_ReturnsTrue()
    {
        // Arrange
        var service = CreateServiceWithClaims(new Claim(ClaimTypes.Role, "admin"));

        // Assert
        Assert.True(service.IsInRole("admin"));
    }

    [Fact]
    public void IsInRole_WithoutMatchingRole_ReturnsFalse()
    {
        // Arrange
        var service = CreateServiceWithClaims(new Claim(ClaimTypes.Role, "teacher"));

        // Assert
        Assert.False(service.IsInRole("admin"));
    }

    private static CurrentUserService CreateServiceWithClaims(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        return new CurrentUserService(httpContextAccessor.Object);
    }
}
