namespace LabLedger.Application.Features.Auth;

public interface IAuthComponent
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
