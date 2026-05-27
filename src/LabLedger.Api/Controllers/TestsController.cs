using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LabLedger.Api.Authorization;
using LabLedger.Api.Extensions;
using LabLedger.Application.Features.Auth;
using LabLedger.Application.Features.Results;
using LabLedger.Application.Features.Tests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabLedger.Api.Controllers;

[ApiController]
[Route("api/tests")]
[Authorize]
public class TestsController : ControllerBase
{
    private readonly ITestComponent _tests;
    private readonly IResultComponent _results;

    public TestsController(ITestComponent tests, IResultComponent results)
    {
        _tests = tests;
        _results = results;
    }

    [HttpPatch("{id:int}/assign")]
    [RequirePermission(Permissions.TestsAssign)]
    public async Task<ActionResult<TestDto>> Assign(
        int id,
        [FromBody] AssignTestRequest request,
        CancellationToken cancellationToken)
    {
        var test = await _tests.AssignAsync(id, request, cancellationToken);
        if (test is null)
            return this.NotFoundProblem();

        return Ok(test);
    }

    [HttpPatch("{id:int}/status")]
    [RequirePermission(Permissions.TestsWrite)]
    public async Task<ActionResult<TestDto>> UpdateStatus(
        int id,
        [FromBody] UpdateTestStatusRequest request,
        CancellationToken cancellationToken)
    {
        var test = await _tests.UpdateStatusAsync(id, request.Status, cancellationToken);
        if (test is null)
            return this.NotFoundProblem();

        return Ok(test);
    }

    [HttpPost("{id:int}/result")]
    [RequirePermission(Permissions.ResultsWrite)]
    public async Task<ActionResult<ResultDto>> CreateResult(
        int id,
        [FromBody] CreateResultRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _results.CreateAsync(id, request, userId, cancellationToken);
        if (result is null)
            return this.NotFoundProblem();

        return CreatedAtRoute("GetResult", new { id = result.Id }, result);
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim is missing.");

        return int.Parse(sub);
    }
}
