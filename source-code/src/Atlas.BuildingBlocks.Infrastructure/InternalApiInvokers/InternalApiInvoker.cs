using System.Net.Http.Json;
using System.Text.Json;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.BuildingBlocks.Application.InternalApiInvokers;

public sealed class InternalApiInvoker(
    IHttpClientFactory httpClientFactory,
    IOptions<InternalApiInvokerOptions> options,
    ILogger<InternalApiInvoker> logger)
    : IInternalApiInvoker
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<InternalApiInvocationResult<TResponse>> InvokeAsync<TResponse>(
        InternalApiInvocationRequest request,
        CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(options.Value.Timeout);

        try
        {
            logger.LogInformation(
                "InternalApiInvoker: {Name} {Method} {Url}",
                request.Name,
                request.Method,
                request.Url);

            var client = httpClientFactory.CreateClient(nameof(InternalApiInvoker));

            using var httpRequest = new HttpRequestMessage(request.Method, request.Url);
            AddHeaders(httpRequest, request);

            if (request.Payload is not null && request.Method != HttpMethod.Get)
                httpRequest.Content = JsonContent.Create(request.Payload, options: JsonOptions);

            using var response = await client.SendAsync(httpRequest, timeoutCts.Token);
            var rawBody = await response.Content.ReadAsStringAsync(timeoutCts.Token);
            var statusCode = (int)response.StatusCode;

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "InternalApiInvoker: {Name} failed with HTTP {StatusCode}. Body={Body}",
                    request.Name,
                    statusCode,
                    rawBody);

                return InternalApiInvocationResult<TResponse>.Failure(
                    statusCode,
                    $"Internal API '{request.Name}' failed with HTTP {statusCode}.",
                    rawBody);
            }

            var value = string.IsNullOrWhiteSpace(rawBody)
                ? default
                : JsonSerializer.Deserialize<TResponse>(rawBody, JsonOptions);

            return InternalApiInvocationResult<TResponse>.Success(statusCode, value, rawBody);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return InternalApiInvocationResult<TResponse>.Failure(
                null,
                $"Internal API '{request.Name}' timed out.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "InternalApiInvoker: {Name} failed unexpectedly", request.Name);

            return InternalApiInvocationResult<TResponse>.Failure(
                null,
                $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private void AddHeaders(HttpRequestMessage httpRequest, InternalApiInvocationRequest request)
    {
        httpRequest.Headers.TryAddWithoutValidation(InternalApiHeaders.ApiKey, options.Value.InternalApiKey);

        if (!string.IsNullOrWhiteSpace(request.CorrelationId))
            httpRequest.Headers.TryAddWithoutValidation(InternalApiHeaders.CorrelationId, request.CorrelationId);

        if (!string.IsNullOrWhiteSpace(request.TraceParent))
            httpRequest.Headers.TryAddWithoutValidation(InternalApiHeaders.TraceParent, request.TraceParent);
    }
}
