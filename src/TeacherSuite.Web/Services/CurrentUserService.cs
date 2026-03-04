using System.Security.Claims;
using TeacherSuite.Application.Common.Interfaces;

namespace TeacherSuite.Web.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("preferred_username")
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public string? Email =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyList<string> Roles =>
        httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList()
            .AsReadOnly()
        ?? (IReadOnlyList<string>)Array.Empty<string>();

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) =>
        httpContextAccessor.HttpContext?.User.IsInRole(role) ?? false;
}
