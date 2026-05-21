using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Atlas.BuildingBlocks.AspNetCore.HttpErrors;

public sealed class ResultToHttpFilter : IResultFilter
{
    private readonly IErrorMessageLocalizer _localizer;

    public ResultToHttpFilter(IErrorMessageLocalizer localizer)
    {
        _localizer = localizer;
    }

    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult objectResult)
            return;

        if (objectResult.Value is not IResponse result)
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
            ErrorCategory.Validation   => StatusCodes.Status400BadRequest,
            ErrorCategory.Business     => StatusCodes.Status422UnprocessableEntity,
            ErrorCategory.Conflict     => StatusCodes.Status409Conflict,
            ErrorCategory.NotFound     => StatusCodes.Status404NotFound,
            ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCategory.Unexpected   => StatusCodes.Status500InternalServerError,
            _                          => StatusCodes.Status500InternalServerError
        };
}
