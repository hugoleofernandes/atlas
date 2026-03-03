using Atlas.API.Observability;
using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Errors;

public static class ValidationProblemDetailsFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var problem = new ApiProblemDetails
        {
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred."
        };

        problem.AddMetadata(
            ErrorCodes.Common.ValidationFailed,
            CorrelationIdMiddleware.Get(context.HttpContext),
            TraceContextHelper.GetTraceId()
        );

        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        problem.Extensions["errors"] = errors;

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status,
            ContentTypes = { "application/problem+json" }
        };
    }
}