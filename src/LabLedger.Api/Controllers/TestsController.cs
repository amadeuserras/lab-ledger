using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using LabLedger.Api.Authorization;
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
        try
        {
            var test = await _tests.AssignAsync(id, request, cancellationToken);
            if (test is null)
                return NotFound();

            return Ok(test);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
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
            return NotFound();

        return Ok(test);
    }

    [HttpPost("{id:int}/result")]
    [RequirePermission(Permissions.ResultsWrite)]
    public async Task<ActionResult<ResultDto>> CreateResult(
        int id,
        [FromBody] CreateResultRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _results.CreateAsync(id, request, userId, cancellationToken);
            if (result is null)
                return NotFound();

            return CreatedAtRoute("GetResult", new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim is missing.");

        return int.Parse(sub);
    }
}
