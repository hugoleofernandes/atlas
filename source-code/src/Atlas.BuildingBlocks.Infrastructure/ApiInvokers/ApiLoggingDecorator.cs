using Microsoft.Extensions.Logging;

namespace Atlas.BuildingBlocks.Application.ApiInvokers;

internal sealed class ApiLoggingDecorator : IApiPipelineStep
{
    private readonly IApiPipelineStep _inner;
    private readonly ILogger<ApiInvoker> _logger;

    public ApiLoggingDecorator(IApiPipelineStep inner, ILoggerFactory loggerFactory)
    {
        _inner = inner;
        _logger = loggerFactory.CreateLogger<ApiInvoker>();
    }

    public async Task<ApiInvocationResult> ExecuteAsync(ApiInvocationRequest request, CancellationToken ct)
    {
        _logger.LogInformation("API invocation started - {Name} {Method} {Url}", request.Name, request.Method, request.Url);

        var result = await _inner.ExecuteAsync(request, ct);

        if (result.IsSuccess)
        {
            _logger.LogInformation("API invocation succeeded - {Name} StatusCode={StatusCode}", request.Name, result.StatusCode);
        }
        else
        {
            _logger.LogWarning(
                "API invocation failed - {Name} StatusCode={StatusCode} Error={Error}",
                request.Name,
                result.StatusCode,
                result.ErrorMessage);
        }

        return result;
    }
}
