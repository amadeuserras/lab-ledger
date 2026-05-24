using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LabLedger.Api.Authorization;
using LabLedger.Application.Features.Auth;
using LabLedger.Application.Features.Samples;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabLedger.Api.Controllers;

[ApiController]
[Route("api/samples")]
[Authorize]
public class SamplesController : ControllerBase
{
    private readonly ISampleComponent _samples;

    public SamplesController(ISampleComponent samples) => _samples = samples;

    [HttpGet]
    [RequirePermission(Permissions.SamplesRead)]
    public async Task<ActionResult<IReadOnlyList<SampleResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var samples = await _samples.GetAllAsync(cancellationToken);
        return Ok(samples);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.SamplesRead)]
    public async Task<ActionResult<SampleResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetByIdAsync(id, cancellationToken);
        if (sample is null)
            return NotFound(new { message = $"Sample {id} was not found." });

        return Ok(sample);
    }

    [HttpPost]
    [RequirePermission(Permissions.SamplesWrite)]
    public async Task<ActionResult<SampleResponse>> Create(
        [FromBody] CreateSampleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _samples.CreateAsync(request, GetCurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (sub is null || !int.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("User id claim is missing or invalid.");

        return userId;
    }
}
