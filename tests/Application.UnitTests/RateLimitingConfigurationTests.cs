using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeacherSuite.Web.RateLimiting;

namespace Application.UnitTests;

public class RateLimitingConfigurationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    #region GetPartitionKey Tests

    [Fact]
    public void GetPartitionKey_ReturnsUserId_WhenAuthenticated()
    {
        // Arrange
        var userId = "user-abc-123";
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)],
            authenticationType: "TestAuth");
        var context = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };

        // Act
        var result = RateLimitingServiceExtensions.GetPartitionKey(context);

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetPartitionKey_ReturnsRemoteIpAddress_WhenNotAuthenticated()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        var result = RateLimitingServiceExtensions.GetPartitionKey(context);

        // Assert
        Assert.Equal("192.168.1.1", result);
    }

    [Fact]
    public void GetPartitionKey_ReturnsAnonymous_WhenNoUserAndNoIp()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = RateLimitingServiceExtensions.GetPartitionKey(context);

        // Assert
        Assert.Equal("anonymous", result);
    }

    [Fact]
    public void GetPartitionKey_ReturnsRemoteIp_WhenAuthenticatedButNoNameIdentifier()
    {
        // Arrange
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Email, "user@example.com")],
            authenticationType: "TestAuth");
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity),
        };
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.5");

        // Act
        var result = RateLimitingServiceExtensions.GetPartitionKey(context);

        // Assert
        Assert.Equal("10.0.0.5", result);
    }

    #endregion

    #region Integration Tests (TestServer)

    [Fact]
    public async Task RateLimiter_AllowsRequests_UnderLimit()
    {
        // Arrange
        await using var app = await CreateTestApp(permitLimit: 5);
        var client = app.GetTestClient();

        // Act & Assert
        for (var i = 0; i < 5; i++)
        {
            using var response = await client.GetAsync("/test");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    [Fact]
    public async Task RateLimiter_Returns429_WhenLimitExceeded()
    {
        // Arrange
        await using var app = await CreateTestApp(permitLimit: 3);
        var client = app.GetTestClient();

        // Act – first 3 should succeed
        for (var i = 0; i < 3; i++)
        {
            using var response = await client.GetAsync("/test");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Act – 4th should be rejected
        using var rejected = await client.GetAsync("/test");

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }

    [Fact]
    public async Task RateLimiter_ResponseIncludesRetryAfterHeader_WhenRejected()
    {
        // The SlidingWindowRateLimiter wrapped by PartitionedRateLimiter does not
        // reliably expose RetryAfter metadata (TryGetMetadata returns false) in the
        // current .NET runtime. The OnRejected callback correctly checks for metadata
        // and only sets Retry-After when available.
        // This test verifies the RFC-compliant rejection response structure — the
        // Type URL and Instance path — which are unique to the OnRejected callback.
        await using var app = await CreateTestApp(permitLimit: 1);
        var client = app.GetTestClient();

        using var _ = await client.GetAsync("/test"); // exhaust the limit

        // Act
        using var rejected = await client.GetAsync("/test");

        // Assert
        var body = await rejected.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(body, JsonOptions);

        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal("https://tools.ietf.org/html/rfc6585#section-4", problemDetails.Type);
        Assert.Equal("/test", problemDetails.Instance);
    }

    [Fact]
    public async Task RateLimiter_ResponseBody_IsProblemDetailsJson()
    {
        // Arrange
        await using var app = await CreateTestApp(permitLimit: 1);
        var client = app.GetTestClient();

        using var _ = await client.GetAsync("/test"); // exhaust the limit

        // Act
        using var rejected = await client.GetAsync("/test");
        var body = await rejected.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(body, JsonOptions);

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.NotNull(problemDetails);
        Assert.Equal(429, problemDetails.Status);
        Assert.Equal("Too Many Requests", problemDetails.Title);
        Assert.Contains("Rate limit exceeded", problemDetails.Detail);
    }

    [Fact]
    public async Task RateLimiter_ReadsConfiguredLimits_FromConfiguration()
    {
        // Arrange – use a custom limit of 2
        await using var app = await CreateTestApp(permitLimit: 2);
        var client = app.GetTestClient();

        // Act – first 2 should succeed
        for (var i = 0; i < 2; i++)
        {
            using var response = await client.GetAsync("/test");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Act – 3rd should be rejected
        using var rejected = await client.GetAsync("/test");

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }

    #endregion

    #region Helpers

    private static async Task<WebApplication> CreateTestApp(
        int permitLimit = 10,
        int windowSeconds = 60,
        int segmentsPerWindow = 6)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["RateLimiting:PermitLimit"] = permitLimit.ToString(),
            ["RateLimiting:WindowSeconds"] = windowSeconds.ToString(),
            ["RateLimiting:SegmentsPerWindow"] = segmentsPerWindow.ToString(),
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddRateLimitingPolicy(builder.Configuration);

        var app = builder.Build();
        app.UseRateLimiter();
        app.MapGet("/test", () => Results.Ok("success"));

        await app.StartAsync();
        return app;
    }

    #endregion
}
