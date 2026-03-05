using System.Security.Claims;
using System.Text.Json;
using TeacherSuite.Web.Auth;

namespace Application.UnitTests;

public class KeycloakClaimsTransformationTests
{
    private readonly KeycloakClaimsTransformation _transformation = new();

    [Fact]
    public async Task TransformAsync_WithRealmRoles_AddsRoleClaims()
    {
        // Arrange
        var realmAccess = JsonSerializer.Serialize(new { roles = new[] { "admin", "teacher" } });
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("realm_access", realmAccess),
        }, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        var roles = result.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("admin", roles);
        Assert.Contains("teacher", roles);
    }

    [Fact]
    public async Task TransformAsync_WithPreferredUsername_AddsNameClaim()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("preferred_username", "john.doe"),
        }, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Assert.Equal("john.doe", result.FindFirstValue(ClaimTypes.Name));
    }

    [Fact]
    public async Task TransformAsync_WithoutRealmAccess_DoesNotAddRoles()
    {
        // Arrange
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("sub", "user-123"),
        }, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Assert.Empty(result.FindAll(ClaimTypes.Role));
    }

    [Fact]
    public async Task TransformAsync_UnauthenticatedUser_ReturnsUnchangedPrincipal()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        Assert.Empty(result.Claims);
    }

    [Fact]
    public async Task TransformAsync_DoesNotDuplicateExistingRoles()
    {
        // Arrange
        var realmAccess = JsonSerializer.Serialize(new { roles = new[] { "admin" } });
        var identity = new ClaimsIdentity(new[]
        {
            new Claim("realm_access", realmAccess),
            new Claim(ClaimTypes.Role, "admin"),
        }, "Bearer");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await _transformation.TransformAsync(principal);

        // Assert
        var roles = result.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Single(roles, "admin");
    }
}
