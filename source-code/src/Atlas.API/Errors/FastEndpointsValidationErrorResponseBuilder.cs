using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.SharedKernel.Application.Errors;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Atlas.API.Errors;

/// <summary>
/// Replaces FastEndpoints' built-in ErrorResponse for both FluentValidation failures and request
/// binding failures (e.g. malformed JSON, enum conversion errors) — these never reach
/// GlobalExceptionMiddleware because FastEndpoints resolves them internally before the endpoint runs.
/// Logs the failure and shapes the response as ApiProblemDetails for consistency with every other
/// error path in the API.
/// </summary>
public static class FastEndpointsValidationErrorResponseBuilder
{
    public static object Build(List<ValidationFailure> failures, HttpContext httpContext, int statusCode)
    {
        var error = CommonErrors.ValidationFailed;

        httpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("FastEndpoints.RequestValidation")
            .LogWarning(
                "Request validation/binding failed for {Path}: {Failures}",
                httpContext.Request.Path,
                string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"))
            );

        var problem = new ApiProblemDetails
        {
            Title = error.FallbackMessage,
            Status = statusCode,
            Detail = "One or more validation errors occurred.",
            Type = $"https://docs.atlas/errors/{error.Code}"
        };

        problem.AddMetadata(
            error.Code,
            CorrelationIdMiddleware.Get(httpContext),
            TraceContextHelper.GetTraceId()
        );

        problem.Extensions["errors"] = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        return problem;
    }
}
