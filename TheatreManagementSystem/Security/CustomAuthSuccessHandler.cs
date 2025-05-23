using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace TheatreManagementSystem.Security
{
    public class CustomAuthSuccessHandler
    {
        public static Task HandleAuthenticationSuccess(HttpContext context, ClaimsPrincipal principal)
        {
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);

            if (roles.Contains("ROLE_ADMIN"))
            {
                // Redirect admin to dashboard
                context.Response.Redirect("/admin/dashboard");
            }
            else if (roles.Contains("ROLE_MANAGER"))
            {
                // Redirect managers to manager dashboard (if exists)
                context.Response.Redirect("/admin/dashboard");
            }
            else
            {
                // Default redirect for regular users
                context.Response.Redirect("/");
            }

            return Task.CompletedTask;
        }

        public static string GetRedirectUrlForUser(ClaimsPrincipal principal)
        {
            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value);

            if (roles.Contains("ROLE_ADMIN"))
            {
                return "/admin/dashboard";
            }
            else if (roles.Contains("ROLE_MANAGER"))
            {
                return "/admin/dashboard";
            }
            else
            {
                return "/";
            }
        }
    }
}