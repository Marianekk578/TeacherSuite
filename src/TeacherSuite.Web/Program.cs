using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TeacherSuite.Application.Common.Interfaces;
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(AuthorizationPolicies.RoleAdmin));
    options.AddPolicy(AuthorizationPolicies.TeacherAccess, policy =>
        policy.RequireRole(AuthorizationPolicies.RoleAdmin, AuthorizationPolicies.RoleTeacher));
    options.AddPolicy(AuthorizationPolicies.SupervisorAccess, policy =>
        policy.RequireRole(AuthorizationPolicies.RoleAdmin, AuthorizationPolicies.RoleSupervisor));
});

builder.Services.AddScoped<AgeGroups>();
builder.Services.AddScoped<Teachers>();
builder.Services.AddScoped<Courses>();
builder.Services.AddScoped<Groups>();
builder.Services.AddScoped<ProgrammingLanguages>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseGlobalExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment()) {
    app.UseRequestLogging();
}

app.MapAgeGroupEndpoints();
app.MapTeacherEndpoints();
app.MapCourseEndpoints();
app.MapGroupEndpoints();
app.MapProgrammingLanguageEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
