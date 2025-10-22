using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Blog.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Blog.Mcv
{
    public class PermissionAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permissionName;

        public PermissionAuthorizeAttribute(string permissionName)
        {
            _permissionName = permissionName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            var dbContext = context.HttpContext.RequestServices.GetRequiredService<BlogDbContext>();

            if (!user.HasPermission(_permissionName, dbContext))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}