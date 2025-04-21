using System.Security.Claims;

namespace Fermion.EntityFramework.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value != null
            ? Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!)
            : null;
}