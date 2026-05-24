using LabLedger.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;

namespace LabLedger.Api.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.HasClaim(AuthClaimTypes.Permission, requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
