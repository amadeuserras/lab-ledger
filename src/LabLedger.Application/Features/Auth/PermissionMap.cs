using LabLedger.DataModel.Entities;

namespace LabLedger.Application.Features.Auth;

public static class PermissionMap
{
    public static IReadOnlyList<string> GetPermissions(UserRole role) =>
        role switch
        {
            UserRole.Scientist =>
            [
                Permissions.SamplesRead,
                Permissions.SamplesWrite
            ],
            UserRole.Technician =>
            [
                Permissions.SamplesRead,
                Permissions.TestsWrite,
                Permissions.TestsAssign,
                Permissions.ResultsWrite
            ],
            UserRole.Supervisor => Permissions.All
                .Where(p => p != Permissions.UsersManage)
                .ToList(),
            UserRole.Admin => Permissions.All,
            _ => []
        };
}
