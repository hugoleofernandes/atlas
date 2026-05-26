using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.BuildingBlocks.AspNetCore.HttpErrors;

public sealed class HttpResultMapper : IHttpResultMapper
{
    private readonly IErrorMessageLocalizer _localizer;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpResultMapper(
        IErrorMessageLocalizer localizer,
        IHttpContextAccessor httpContextAccessor)
    {
        _localizer = localizer;
        _httpContextAccessor = httpContextAccessor;
    }

    public IActionResult ToOkResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new ObjectResult(result.Value)
        {
            StatusCode = StatusCodes.Status200OK
        };
    }

    public IActionResult ToCreatedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new ObjectResult(map(result.Value!))
        {
            StatusCode = StatusCodes.Status201Created
        };
    }

    public IActionResult ToCreateAcceptedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new ObjectResult(map(result.Value!))
        {
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public IActionResult ToUpdatedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new ObjectResult(map(result.Value!))
        {
            StatusCode = StatusCodes.Status200OK
        };
    }

    public IActionResult ToUpdatedNoContentResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new NoContentResult();
    }

    public IActionResult ToUpdateAcceptedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new ObjectResult(map(result.Value!))
        {
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public IActionResult ToDeletedResult<T>(Result<T> result)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new NoContentResult();
    }

    public IActionResult ToDeletedWithBodyResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new ObjectResult(map(result.Value!))
        {
            StatusCode = StatusCodes.Status200OK
        };
    }

    public IActionResult ToDeleteAcceptedResult<TOutput, TResponse>(
        Result<TOutput> result,
        Func<TOutput, TResponse> map)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        return new ObjectResult(map(result.Value!))
        {
            StatusCode = StatusCodes.Status202Accepted
        };
    }

    public IActionResult ToCreatedAtActionResult<TOutput, TResponse>(
        ControllerBase controller,
        Result<TOutput> result,
        string actionName,
        Func<TOutput, object?> routeValues,
        Func<TOutput, TResponse> map)
    {
        if (!result.IsSuccess)
            return ToProblemResult(result.ErrorDefinition!, result.Error);

        var value = result.Value!;
        return controller.CreatedAtAction(actionName, routeValues(value), map(value));
    }

    public ObjectResult ToProblemResult(ErrorDefinition error, string? detail = null)
    {
        var problem = new ApiProblemDetails
        {
            Title = _localizer.Localize(error),
            Status = MapCategory(error.Category),
            Detail = detail,
            Type = $"https://docs.atlas/errors/{error.Code}"
        };

        problem.AddMetadata(
            error.Code,
            GetCorrelationId(),
            TraceContextHelper.GetTraceId());

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status,
            ContentTypes = { "application/problem+json" }
        };
    }

    private string? GetCorrelationId()
    {
        var context = _httpContextAccessor.HttpContext;
        return context is null ? null : CorrelationIdMiddleware.Get(context);
    }

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
