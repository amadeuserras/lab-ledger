namespace LabLedger.Application.Features.Auth;

public class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
