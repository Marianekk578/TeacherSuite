using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace TeacherSuite.Web.Auth;

public class KeycloakClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated)
        {
            return Task.FromResult(principal);
        }

        MapRealmRoles(identity);
        MapPreferredUsername(identity);

        return Task.FromResult(principal);
    }

    private static void MapRealmRoles(ClaimsIdentity identity)
    {
        try
        {
            var realmAccessClaim = identity.FindFirst("realm_access");
            if (realmAccessClaim == null)
            {
                return;
            }

            using var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);
            if (!realmAccess.RootElement.TryGetProperty("roles", out var roles))
            {
                return;
            }

            foreach (var role in roles.EnumerateArray())
            {
                var roleValue = role.GetString();
                if (!string.IsNullOrEmpty(roleValue) &&
                    !identity.HasClaim(ClaimTypes.Role, roleValue))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleValue));
                }
            }
        }
        catch (JsonException)
        {
            // Malformed or unexpected JSON in realm_access claim; ignore and do not add role claims.
        }
    }

    private static void MapPreferredUsername(ClaimsIdentity identity)
    {
        var preferredUsername = identity.FindFirst("preferred_username");
        if (preferredUsername != null && !identity.HasClaim(c => c.Type == ClaimTypes.Name))
        {
            identity.AddClaim(new Claim(ClaimTypes.Name, preferredUsername.Value));
        }
    }
}
