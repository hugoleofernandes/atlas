using Atlas.API.Observability;
using Atlas.SharedKernel.Application;

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

            var problem = new ApiProblemDetails
            {
                Title = "Unexpected error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred."
            };

            problem.AddMetadata(
                ErrorCodes.Common.UnexpectedError,
                CorrelationIdMiddleware.Get(context),
                TraceContextHelper.GetTraceId()
            );

            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}