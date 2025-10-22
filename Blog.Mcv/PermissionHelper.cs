using System.Security.Claims;
using System.Text.Json;
using Blog.Data.Entites;
using Microsoft.EntityFrameworkCore;
using Blog.Data;

namespace Blog.Mcv
{
    public static class PermissionHelper
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permissionName, BlogDbContext dbContext)
        {
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return false;
            }

            if (user.IsInRole("admin"))
            {
                return true; // Admin users have all permissions
            }

            var userIdClaim = user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return false;
            }

            var userEntity = dbContext.Set<UserEntity>()
                                    .Include(u => u.Role)
                                    .FirstOrDefault(u => u.Id == userId);

            if (userEntity?.Role == null || string.IsNullOrEmpty(userEntity.Role.PermissionsJson))
            {
                return false;
            }

            var permissions = JsonSerializer.Deserialize<List<string>>(userEntity.Role.PermissionsJson);
            return permissions?.Contains(permissionName) == true;
        }
    }
}