using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TheatreManagementSystem.Models;

namespace TheatreManagementSystem.Security
{
    // Custom authorization requirement for roles
    public class RoleRequirement : IAuthorizationRequirement
    {
        public UserRole[] AllowedRoles { get; }

        public RoleRequirement(params UserRole[] allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }
    }

    // Authorization handler for role requirements
    public class RoleAuthorizationHandler : AuthorizationHandler<RoleRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            if (Enum.TryParse<UserRole>(userRole, out var role) && requirement.AllowedRoles.Contains(role))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }

    // Custom authorization attributes
    public class AdminOnlyAttribute : AuthorizeAttribute
    {
        public AdminOnlyAttribute()
        {
            Policy = "AdminOnly";
        }
    }

    public class AdminOrManagerAttribute : AuthorizeAttribute
    {
        public AdminOrManagerAttribute()
        {
            Policy = "AdminOrManager";
        }
    }

    public class AuthenticatedUserAttribute : AuthorizeAttribute
    {
        public AuthenticatedUserAttribute()
        {
            Policy = "AuthenticatedUser";
        }
    }

    // Extension methods for authorization policy setup
    public static class AuthorizationPolicyExtensions
    {
        public static void AddCustomAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.Requirements.Add(new RoleRequirement(UserRole.ROLE_ADMIN)));

                options.AddPolicy("AdminOrManager", policy =>
                    policy.Requirements.Add(new RoleRequirement(UserRole.ROLE_ADMIN, UserRole.ROLE_MANAGER)));

                options.AddPolicy("AuthenticatedUser", policy =>
                    policy.Requirements.Add(new RoleRequirement(UserRole.ROLE_USER, UserRole.ROLE_MANAGER, UserRole.ROLE_ADMIN)));
            });

            services.AddScoped<IAuthorizationHandler, RoleAuthorizationHandler>();
        }
    }
}