using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.AspNetCore.HttpErrors;

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

    public async Task Invoke(HttpContext context, IErrorMessageLocalizer localizer)
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
                Title = localizer.Localize(error),
                Status = ex.Category.ToHttpStatus(),
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
                Title = localizer.Localize(error),
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

}



