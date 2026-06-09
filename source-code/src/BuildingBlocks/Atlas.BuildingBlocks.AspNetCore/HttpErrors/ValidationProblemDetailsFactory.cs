using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.BuildingBlocks.AspNetCore.HttpErrors;

public static class ValidationProblemDetailsFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var error = CommonErrors.ValidationFailed;

        var problem = new ApiProblemDetails
        {
            Title = error.FallbackMessage,
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred.",
            Type = $"https://docs.atlas/errors/{error.Code}"
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
            StatusCode = StatusCodes.Status400BadRequest,
            ContentTypes = { "application/problem+json" }
        };
    }
}



