using System.Security.Claims;
using ElearningAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ElearningAPI.Helpers
{
    public static class KeycloakClaimsHelper
    {
        public static async Task<int> GetUserIdAsync(
            ClaimsPrincipal user, ElearningDbContext db)
        {
            var username = user.FindFirstValue("preferred_username")
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? "";

            if (string.IsNullOrEmpty(username)) return 0;

            var dbUser = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);
            return dbUser?.Id ?? 0;
        }

        public static string GetUsername(ClaimsPrincipal user)
        {
            return user.FindFirstValue("preferred_username")
                ?? user.FindFirstValue(ClaimTypes.Name)
                ?? "";
        }

        public static string GetRole(ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Role)
                ?? user.FindFirstValue("roles")
                ?? "";
        }
    }
}