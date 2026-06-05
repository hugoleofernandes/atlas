namespace Atlas.BuildingBlocks.Application.InternalApiInvokers;

public interface IInternalApiInvoker
{
    Task<InternalApiInvocationResult<TResponse>> InvokeAsync<TResponse>(
        InternalApiInvocationRequest request,
        CancellationToken ct);
}
