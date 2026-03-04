using Atlas.API.Observability;
using Atlas.SharedKernel.Application.Errors;

namespace Atlas.API.Errors;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            var error = new ErrorDefinition(
                Code: "COMMON_999",
                DefaultMessage: "Unexpected error",
                Category: ErrorCategory.Unexpected
            );

            var problem = new ApiProblemDetails
            {
                Title = error.DefaultMessage,
                Status = MapCategory(error.Category),
                Detail = "An unexpected error occurred.",
                Type = $"https://docs.atlas/errors/{error.Code.ToLower()}"
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
            ErrorCategory.Validation => StatusCodes.Status400BadRequest,
            ErrorCategory.Business => StatusCodes.Status422UnprocessableEntity,
            ErrorCategory.Conflict => StatusCodes.Status409Conflict,
            ErrorCategory.NotFound => StatusCodes.Status404NotFound,
            ErrorCategory.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorCategory.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
}