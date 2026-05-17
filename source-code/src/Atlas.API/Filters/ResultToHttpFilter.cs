using Atlas.API.Errors;
using Atlas.API.Observability;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Atlas.API.Filters;

public sealed class ResultToHttpFilter : IResultFilter
{
    private readonly ErrorMessageLocalizer _localizer;

    public ResultToHttpFilter(ErrorMessageLocalizer localizer)
    {
        _localizer = localizer;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult objectResult)
            return;

        if (objectResult.Value is not SharedKernel.Application.IResponse result)
            return;

        if (!result.IsSuccess)
        {
            var error = result.ErrorDefinition!;

            var problem = new ApiProblemDetails
            {
                Title = _localizer.Localize(error),
                Status = MapCategory(error.Category),
                Detail = result.Error,
                Type = $"https://docs.atlas/errors/{error.Code}"
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
            ErrorCategory.Validation  => StatusCodes.Status400BadRequest,
            ErrorCategory.Business    => StatusCodes.Status422UnprocessableEntity,
            ErrorCategory.Conflict    => StatusCodes.Status409Conflict,
            ErrorCategory.NotFound    => StatusCodes.Status404NotFound,
            ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCategory.Unexpected  => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
}
