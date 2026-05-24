using LabLedger.Application.Features.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabLedger.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthComponent _auth;

    public AuthController(IAuthComponent auth) => _auth = auth;

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _auth.RegisterAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _auth.LoginAsync(request, cancellationToken);
        if (response is null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(response);
    }
}
