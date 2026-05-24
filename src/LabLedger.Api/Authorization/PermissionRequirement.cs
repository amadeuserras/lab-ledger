using Microsoft.AspNetCore.Authorization;

namespace LabLedger.Api.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission) => Permission = permission;

    public string Permission { get; }
}
