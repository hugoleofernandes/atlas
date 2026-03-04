using Atlas.API.Observability;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Errors;

public static class ValidationProblemDetailsFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var error = new ErrorDefinition(
            Code: "COMMON_001",
            DefaultMessage: "Validation failed",
            Category: ErrorCategory.Validation
        );

        var problem = new ApiProblemDetails
        {
            Title = error.DefaultMessage,
            Status = 400,
            Detail = "One or more validation errors occurred.",
            Type = $"https://docs.atlas/errors/{error.Code.ToLower()}"
        };

        problem.AddMetadata(
            error.Code,
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
            StatusCode = 400,
            ContentTypes = { "application/problem+json" }
        };
    }
}