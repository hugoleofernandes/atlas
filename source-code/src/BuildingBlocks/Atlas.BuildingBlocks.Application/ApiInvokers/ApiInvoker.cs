using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.BuildingBlocks.Application.ApiInvokers;

public sealed class ApiInvoker : IApiInvoker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptions<ApiInvokerOptions> _options;

    public ApiInvoker(
        IHttpClientFactory httpClientFactory,
        ILoggerFactory loggerFactory,
        IOptions<ApiInvokerOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _loggerFactory = loggerFactory;
        _options = options;
    }

    public Task<ApiInvocationResult> InvokeAsync(ApiInvocationRequest request, CancellationToken ct)
    {
        IApiPipelineStep pipeline = new ApiHttpPipelineStep(_httpClientFactory, _options);
        pipeline = new ApiErrorMappingDecorator(pipeline);
        pipeline = new ApiAuthenticationDecorator(pipeline, _options);
        pipeline = new ApiTracePropagationDecorator(pipeline);
        pipeline = new ApiLoggingDecorator(pipeline, _loggerFactory);
        pipeline = new ApiTelemetryDecorator(pipeline);

        return pipeline.ExecuteAsync(request, ct);
    }
}
