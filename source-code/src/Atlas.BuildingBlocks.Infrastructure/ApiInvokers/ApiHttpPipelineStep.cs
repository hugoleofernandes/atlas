using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace Atlas.BuildingBlocks.Application.ApiInvokers;

internal sealed class ApiHttpPipelineStep : IApiPipelineStep
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<ApiInvokerOptions> _options;

    public ApiHttpPipelineStep(IHttpClientFactory httpClientFactory, IOptions<ApiInvokerOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task<ApiInvocationResult> ExecuteAsync(ApiInvocationRequest request, CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(_options.Value.Timeout);

        var client = _httpClientFactory.CreateClient(nameof(ApiInvoker));

        using var httpRequest = new HttpRequestMessage(request.Method, request.Url)
        {
            Content = JsonContent.Create(request.Payload)
        };

        foreach (var (key, value) in ApiInvocationHeaders.Current)
        {
            if (!string.IsNullOrWhiteSpace(value))
                httpRequest.Headers.TryAddWithoutValidation(key, value);
        }

        using var response = await client.SendAsync(httpRequest, timeoutCts.Token);
        var content = await response.Content.ReadAsStringAsync(timeoutCts.Token);

        return response.IsSuccessStatusCode
            ? ApiInvocationResult.Success((int)response.StatusCode)
            : ApiInvocationResult.Failure((int)response.StatusCode, content);
    }
}
