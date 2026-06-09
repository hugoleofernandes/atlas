namespace Atlas.BuildingBlocks.Application.ApiInvokers;

public interface IApiInvoker
{
    Task<ApiInvocationResult> InvokeAsync(ApiInvocationRequest request, CancellationToken ct);
}
