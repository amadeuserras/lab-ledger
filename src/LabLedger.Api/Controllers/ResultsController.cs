using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LabLedger.Api.Authorization;
using LabLedger.Api.Extensions;
using LabLedger.Application.Features.Auth;
using LabLedger.Application.Features.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabLedger.Api.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly IResultComponent _results;

    public ResultsController(IResultComponent results) => _results = results;

    [HttpGet("{id:int}", Name = "GetResult")]
    [RequirePermission(Permissions.SamplesRead)]
    public async Task<ActionResult<ResultDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _results.GetByIdAsync(id, cancellationToken);
        if (result is null)
            return this.NotFoundProblem();

        return Ok(result);
    }

    [HttpPatch("{id:int}/publish")]
    [RequirePermission(Permissions.ResultsPublish)]
    public async Task<ActionResult<ResultDto>> Publish(int id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var result = await _results.PublishAsync(id, userId, cancellationToken);
        if (result is null)
            return this.NotFoundProblem();

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User id claim is missing.");

        return int.Parse(sub);
    }
}
