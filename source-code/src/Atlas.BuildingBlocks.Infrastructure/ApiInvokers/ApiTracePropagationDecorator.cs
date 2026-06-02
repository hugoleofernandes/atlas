using Atlas.SharedKernel.Application;

namespace Atlas.BuildingBlocks.Application.ApiInvokers;

internal sealed class ApiTracePropagationDecorator : IApiPipelineStep
{
    private readonly IApiPipelineStep _inner;

    public ApiTracePropagationDecorator(IApiPipelineStep inner)
    {
        _inner = inner;
    }

    public Task<ApiInvocationResult> ExecuteAsync(ApiInvocationRequest request, CancellationToken ct)
    {
        ApiInvocationHeaders.Set(InternalApiHeaders.IdempotencyKey, request.IdempotencyKey.ToString());
        ApiInvocationHeaders.Set(InternalApiHeaders.OutboxMessageId, request.OutboxMessageId.ToString());
        ApiInvocationHeaders.Set(InternalApiHeaders.OutboxSubscription, request.Name);
        ApiInvocationHeaders.Set(InternalApiHeaders.CorrelationId, request.CorrelationId);
        ApiInvocationHeaders.Set(InternalApiHeaders.TenantId, request.TenantId.ToString());
        ApiInvocationHeaders.Set(InternalApiHeaders.UserId, request.UserId.ToString());
        ApiInvocationHeaders.Set(InternalApiHeaders.UserEmail, request.UserEmail);
        ApiInvocationHeaders.Set(InternalApiHeaders.TraceParent, request.TraceParent);

        return _inner.ExecuteAsync(request, ct);
    }
}
