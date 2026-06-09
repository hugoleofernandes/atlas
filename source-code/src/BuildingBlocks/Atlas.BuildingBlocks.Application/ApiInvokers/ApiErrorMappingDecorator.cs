namespace Atlas.BuildingBlocks.Application.ApiInvokers;

internal sealed class ApiErrorMappingDecorator : IApiPipelineStep
{
    private readonly IApiPipelineStep _inner;

    public ApiErrorMappingDecorator(IApiPipelineStep inner)
    {
        _inner = inner;
    }

    public async Task<ApiInvocationResult> ExecuteAsync(ApiInvocationRequest request, CancellationToken ct)
    {
        try
        {
            return await _inner.ExecuteAsync(request, ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return ApiInvocationResult.Failure(null, "HTTP invocation timed out.");
        }
        catch (Exception ex)
        {
            return ApiInvocationResult.Failure(null, $"{ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            ApiInvocationHeaders.Clear();
        }
    }
}
