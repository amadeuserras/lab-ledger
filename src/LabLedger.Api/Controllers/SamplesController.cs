using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LabLedger.Api.Authorization;
using LabLedger.Api.Extensions;
using LabLedger.Application.Features.Auth;
using LabLedger.Application.Features.Samples;
using LabLedger.Application.Features.Tests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabLedger.Api.Controllers;

[ApiController]
[Route("api/samples")]
[Authorize]
public class SamplesController : ControllerBase
{
    private readonly ISampleComponent _samples;
    private readonly ITestComponent _tests;

    public SamplesController(ISampleComponent samples, ITestComponent tests)
    {
        _samples = samples;
        _tests = tests;
    }

    [HttpGet]
    [RequirePermission(Permissions.SamplesRead)]
    public async Task<ActionResult<IReadOnlyList<SampleDto>>> GetAll(SampleStatus? status, CancellationToken cancellationToken)
    {
        var samples = await _samples.GetAllAsync(status, cancellationToken);
        return Ok(samples);
    }

    [HttpGet("{id:int}")]
    [RequirePermission(Permissions.SamplesRead)]
    public async Task<ActionResult<SampleDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var sample = await _samples.GetByIdAsync(id, cancellationToken);
        if (sample is null)
            return this.NotFoundProblem();

        return Ok(sample);
    }

    [HttpPost]
    [RequirePermission(Permissions.SamplesWrite)]
    public async Task<ActionResult<SampleDto>> Create(
        [FromBody] CreateSampleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var sample = await _samples.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = sample.Id }, sample);
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
            return this.NotFoundProblem();

        return Ok(sample);
    }

    [HttpPost("{id:int}/tests")]
    [RequirePermission(Permissions.TestsWrite)]
    public async Task<ActionResult<TestDto>> CreateTest(
        int id,
        [FromBody] CreateTestRequest request,
        CancellationToken cancellationToken)
    {
        var test = await _tests.CreateAsync(id, request, cancellationToken);
        if (test is null)
            return this.NotFoundProblem();

        return CreatedAtAction(nameof(CreateTest), new { id = test.Id }, test);
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim is missing.");

        return int.Parse(sub);
    }
}
