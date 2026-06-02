using Microsoft.Extensions.Options;
using Atlas.SharedKernel.Application;

namespace Atlas.BuildingBlocks.Application.ApiInvokers;

internal sealed class ApiAuthenticationDecorator : IApiPipelineStep
{
    private readonly IApiPipelineStep _inner;
    private readonly IOptions<ApiInvokerOptions> _options;

    public ApiAuthenticationDecorator(IApiPipelineStep inner, IOptions<ApiInvokerOptions> options)
    {
        _inner = inner;
        _options = options;
    }

    public Task<ApiInvocationResult> ExecuteAsync(ApiInvocationRequest request, CancellationToken ct)
    {
        ApiInvocationHeaders.Set(InternalApiHeaders.ApiKey, _options.Value.InternalApiKey);
        return _inner.ExecuteAsync(request, ct);
    }
}
