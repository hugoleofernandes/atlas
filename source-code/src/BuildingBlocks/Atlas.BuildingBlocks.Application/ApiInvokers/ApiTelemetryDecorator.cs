using System.Diagnostics;

namespace Atlas.BuildingBlocks.Application.ApiInvokers;

internal sealed class ApiTelemetryDecorator : IApiPipelineStep
{
    private static readonly ActivitySource ActivitySource = new("Atlas.ApiInvoker");
    private readonly IApiPipelineStep _inner;

    public ApiTelemetryDecorator(IApiPipelineStep inner)
    {
        _inner = inner;
    }

    public async Task<ApiInvocationResult> ExecuteAsync(ApiInvocationRequest request, CancellationToken ct)
    {
        using var activity = ActivitySource.StartActivity("api.invoke", ActivityKind.Client);
        activity?.SetTag("api.invocation.name", request.Name);
        activity?.SetTag("http.request.method", request.Method.Method);
        activity?.SetTag("url.full", request.Url.ToString());

        var result = await _inner.ExecuteAsync(request, ct);

        activity?.SetTag("http.response.status_code", result.StatusCode);
        activity?.SetStatus(result.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error, result.ErrorMessage);

        return result;
    }
}
