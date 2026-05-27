using Microsoft.AspNetCore.Mvc;

namespace LabLedger.Api.Extensions;

public static class ControllerExtensions
{
    public static ActionResult NotFoundProblem(
        this ControllerBase controller,
        string? detail = null) =>
        controller.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: detail ?? "The requested resource was not found.");
}
