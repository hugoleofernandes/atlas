using Atlas.API.Observability;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;

namespace Atlas.API.Errors;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly ErrorMessageLocalizer _localizer;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        ErrorMessageLocalizer localizer)
    {
        _next = next;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception: {ErrorCode}", ex.ErrorCode);

            var error = new ErrorDefinition(ex.ErrorCode, ex.Message, ex.Category);

            var problem = new ApiProblemDetails
            {
                Title = _localizer.Localize(error),
                Status = MapCategory(ex.Category),
                Detail = ex.Message,
                Type = $"https://docs.atlas/errors/{ex.ErrorCode}"
            };

            problem.AddMetadata(
                ex.ErrorCode,
                CorrelationIdMiddleware.Get(context),
                TraceContextHelper.GetTraceId()
            );

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            var error = CommonErrors.Unexpected;

            var problem = new ApiProblemDetails
            {
                Title = _localizer.Localize(error),
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred. Please try again later.",
                Type = $"https://docs.atlas/errors/{error.Code}"
            };

            problem.AddMetadata(
                error.Code,
                CorrelationIdMiddleware.Get(context),
                TraceContextHelper.GetTraceId()
            );

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problem);
        }
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
