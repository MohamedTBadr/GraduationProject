using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers
{
    [ApiController]
    public class APIController : ControllerBase
    {
        // ─── Raw extractors (still throw — use in [Authorize] endpoints only) ───

        protected Guid GetUserIdFromToken()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null || !Guid.TryParse(claim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user id in token.");

            return userId;
        }

        protected string GetUserRoleFromToken()
        {
            var claim = User.FindFirst(ClaimTypes.Role)
                     ?? User.FindFirst("role");

            if (claim == null)
                throw new UnauthorizedAccessException("Invalid or missing role in token.");

            return claim.Value;
        }

        // ─── Safe helpers (never throw — use in public/hybrid endpoints) ────────

        protected Guid? TryGetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
        }

        protected string? TryGetRole()
        {
            return (User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role"))?.Value;
        }

        // ─── Role checks (safe — default to false for anonymous users) ──────────

        protected bool IsAdmin() => TryGetRole() == "Admin";
        protected bool IsVendor() => TryGetRole() == "Vendor";
        protected bool IsClient() => TryGetRole() == "Customer";

        protected bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

        /// <summary>
        /// Returns true only if current user is Admin OR the requested resource belongs to them.
        /// Requires authentication — call only from [Authorize] endpoints.
        /// </summary>
        protected bool IsAdminOrOwner(Guid resourceOwnerId)
            => IsAdmin() || TryGetUserId() == resourceOwnerId;
    }
}