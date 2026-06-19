using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Domain.Common;
using TeacherSuite.Infrastructure;
using TeacherSuite.Web.Auth;
using TeacherSuite.Web.Endpoints;
using TeacherSuite.Web.Middleware;
using TeacherSuite.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var partitionKey = httpContext.User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(partitionKey))
        {
            partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
        }

        return RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey,
            _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromSeconds(60),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                AutoReplenishment = true,
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        var retryAfter = TimeSpan.FromSeconds(10);
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterMetadata))
        {
            retryAfter = retryAfterMetadata;
        }

        context.HttpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                status = StatusCodes.Status429TooManyRequests,
                title = "Too many requests",
                detail = "Rate limit exceeded. Please retry after the number of seconds specified in the Retry-After header.",
            },
            cancellationToken);
    };
});

var spaOrigin = builder.Configuration["Cors:SpaOrigin"]
    ?? (builder.Environment.IsDevelopment()
        ? "http://localhost:4200"
        : throw new InvalidOperationException("Cors:SpaOrigin missing."));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(spaOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"] ?? "http://localhost:8081";
        var realm = builder.Configuration["Keycloak:Realm"] ?? "teachersuite";

        options.Authority = $"{keycloakBaseUrl}/realms/{realm}";
        options.Audience = builder.Configuration["Keycloak:Audience"] ?? "account";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = $"{keycloakBaseUrl}/realms/{realm}",
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(AppRoles.Admin))
    .AddPolicy(AuthorizationPolicies.SupervisorAccess, policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Supervisor))
    .AddPolicy(AuthorizationPolicies.TeacherAccess, policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Supervisor, AppRoles.Teacher))
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddScoped<AgeGroups>();
builder.Services.AddScoped<Teachers>();
builder.Services.AddScoped<Courses>();
builder.Services.AddScoped<Groups>();
builder.Services.AddScoped<ProgrammingLanguages>();
builder.Services.AddScoped<Students>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseGlobalExceptionHandler();

app.UseCors();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

if (app.Environment.IsDevelopment()) {
    app.UseRequestLogging();
}

app.MapAgeGroupEndpoints();
app.MapTeacherEndpoints();
app.MapCourseEndpoints();
app.MapGroupEndpoints();
app.MapProgrammingLanguageEndpoints();
app.MapStudentEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
}

app.Run();
