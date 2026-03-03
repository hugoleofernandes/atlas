using Atlas.API.Errors;
using Atlas.API.Observability;
using Atlas.SharedKernel.Application;
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
            var problem = new ApiProblemDetails
            {
                Title = result.Error ?? "Application error",
                Status = MapStatus(result.ErrorCode),
                Detail = result.Error
            };

            problem.AddMetadata(
                result.ErrorCode,
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

        // SUCCESS
        var valueProperty = result.GetType().GetProperty("Value");
        var value = valueProperty?.GetValue(result);

        context.Result = new ObjectResult(value)
        {
            StatusCode = objectResult.StatusCode ?? 200
        };
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static int MapStatus(string? errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.Staff.AlreadyExists => StatusCodes.Status409Conflict,
            ErrorCodes.Common.ValidationFailed => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };
    }
}