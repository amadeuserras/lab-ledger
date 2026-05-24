using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
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
    public async Task<ActionResult<IReadOnlyList<SampleDto>>> GetAll(CancellationToken cancellationToken)
    {
        var samples = await _samples.GetAllAsync(cancellationToken);
        return Ok(samples);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.SamplesRead)]
    public async Task<ActionResult<SampleDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetByIdAsync(id, cancellationToken);
        if (sample is null)
            return NotFound();

        return Ok(sample);
    }

    [HttpPost]
    [RequirePermission(Permissions.SamplesWrite)]
    public async Task<ActionResult<SampleDto>> Create(
        [FromBody] CreateSampleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var sample = await _samples.CreateAsync(request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = sample.Id }, sample);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.SamplesWrite)]
    public async Task<ActionResult<SampleDto>> UpdateStatus(
        int id,
        [FromBody] UpdateSampleStatusRequest request,
        CancellationToken cancellationToken)
    {
        var sample = await _samples.UpdateStatusAsync(id, request.Status, cancellationToken);
        if (sample is null)
            return NotFound();

        return Ok(sample);
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim is missing.");

        return int.Parse(sub);
    }
}
