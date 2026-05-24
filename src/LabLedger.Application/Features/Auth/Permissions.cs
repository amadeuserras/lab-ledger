namespace LabLedger.Application.Features.Auth;

public static class Permissions
{
    public const string SamplesRead = "Samples:Read";
    public const string SamplesWrite = "Samples:Write";
    public const string TestsWrite = "Tests:Write";
    public const string TestsAssign = "Tests:Assign";
    public const string ResultsWrite = "Results:Write";
    public const string ResultsPublish = "Results:Publish";
    public const string UsersManage = "Users:Manage";

    public static IReadOnlyList<string> All { get; } =
    [
        SamplesRead,
        SamplesWrite,
        TestsWrite,
        TestsAssign,
        ResultsWrite,
        ResultsPublish,
        UsersManage
    ];
}
