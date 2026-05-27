using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LabLedger.Api.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment,
        ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger;
        _environment = environment;
        _problemDetailsFactory = problemDetailsFactory;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validationException => CreateValidationProblem(httpContext, validationException),
            UnauthorizedAccessException unauthorizedException =>
                CreateProblem(httpContext, StatusCodes.Status403Forbidden, "Forbidden", unauthorizedException.Message),
            InvalidOperationException conflictException =>
                CreateProblem(httpContext, StatusCodes.Status409Conflict, "Conflict", conflictException.Message),
            _ => CreateUnhandledProblem(httpContext, exception)
        };

        if (problem.Status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = problem.Status!.Value;

        var problemDetailsService = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });

        return true;
    }

    private ProblemDetails CreateValidationProblem(
        HttpContext httpContext,
        ValidationException exception)
    {
        var problem = CreateProblem(
            httpContext,
            StatusCodes.Status400BadRequest,
            "Validation Error",
            "One or more validation errors occurred.");

        problem.Extensions["errors"] = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return problem;
    }

    private ProblemDetails CreateUnhandledProblem(HttpContext httpContext, Exception exception)
    {
        var detail = _environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred.";

        return CreateProblem(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Internal Server Error",
            detail);
    }

    private ProblemDetails CreateProblem(
        HttpContext httpContext,
        int statusCode,
        string title,
        string? detail)
    {
        return _problemDetailsFactory.CreateProblemDetails(
            httpContext,
            statusCode,
            title: title,
            detail: detail);
    }
}
