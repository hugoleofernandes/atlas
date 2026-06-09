namespace Atlas.BuildingBlocks.Application.ApiInvokers;

internal interface IApiPipelineStep
{
    Task<ApiInvocationResult> ExecuteAsync(ApiInvocationRequest request, CancellationToken ct);
}
