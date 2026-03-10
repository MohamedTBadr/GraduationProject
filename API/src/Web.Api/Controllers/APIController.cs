using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected Guid GetUserIdFromToken()
        {
            // 🔍 Debug - print all claims to see what's there
            foreach (var claims in User.Claims)
            {
                Console.WriteLine($"Type: {claims.Type} | Value: {claims.Value}");
            }
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

            return claim.Value; // "Admin" | "Vendor" | "Client"
        }

        protected bool IsAdmin() => GetUserRoleFromToken() == "Admin";
        protected bool IsVendor() => GetUserRoleFromToken() == "Vendor";
        protected bool IsClient() => GetUserRoleFromToken() == "Client";

        /// <summary>
        /// Returns true only if current user is Admin OR the requested resource belongs to them.
        /// </summary>
        protected bool IsAdminOrOwner(Guid resourceOwnerId)
            => IsAdmin() || GetUserIdFromToken() == resourceOwnerId;
    }
}