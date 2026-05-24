namespace LabLedger.Application.Features.Auth;

public record RegisterRequest(string Email, string Password, string FullName);

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, int UserId, string Email, string Role);

public record RegisterResponse(int UserId, string Email, string FullName, string Role);
