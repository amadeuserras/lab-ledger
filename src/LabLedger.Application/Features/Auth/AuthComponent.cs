using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LabLedger.Core.Interfaces;
using LabLedger.DataModel.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace LabLedger.Application.Features.Auth;

public class AuthComponent : IAuthComponent
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;

    public AuthComponent(IUnitOfWork unitOfWork, IOptions<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.GetRepository<User>();
        var query = userRepo.Query();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        bool emailExists = await query.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName.Trim(),
            Role = UserRole.Scientist,
            CreatedAt = DateTime.UtcNow
        };
            
        await userRepo.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id, user.Email, user.FullName, user.Role.ToString());
    }

    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var userRepo = _unitOfWork.GetRepository<User>();
        var query = userRepo.Query();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await query.SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = CreateToken(user);
        return new LoginResponse(token, user.Id, user.Email, user.Role.ToString());
    }

    private string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(AuthClaimTypes.Role, user.Role.ToString())
        };

        foreach (var permission in PermissionMap.GetPermissions(user.Role))
            claims.Add(new Claim(AuthClaimTypes.Permission, permission));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
