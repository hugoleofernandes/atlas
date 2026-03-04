using Atlas.API.Errors;
using Atlas.API.Observability;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Atlas.API.Filters;

public sealed class ResultToHttpFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult objectResult)
            return;

        if (objectResult.Value is not SharedKernel.Application.IResult result)
            return;

        if (!result.Success)
        {
            var error = result.ErrorDefinition!;

            var problem = new ApiProblemDetails
            {
                Title = error.DefaultMessage,
                Status = MapCategory(error.Category),
                Detail = result.Error,
                Type = $"https://docs.atlas/errors/{error.Code.ToLower()}"
            };

            problem.AddMetadata(
                error.Code,
                CorrelationIdMiddleware.Get(context.HttpContext),
                TraceContextHelper.GetTraceId()
            );

            context.Result = new ObjectResult(problem)
            {
                StatusCode = problem.Status,
                ContentTypes = { "application/problem+json" }
            };

            return;
        }

        context.Result = new ObjectResult(result.GetValue())
        {
            StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK
        };
    }

    public void OnResultExecuted(ResultExecutedContext context) { }

    private static int MapCategory(ErrorCategory category)
        => category switch
        {
            ErrorCategory.Validation => 400,
            ErrorCategory.Conflict => 409,
            ErrorCategory.NotFound => 404,
            ErrorCategory.Unauthorized => 401,
            ErrorCategory.Unexpected => 500,
            _ => 400
        };
}