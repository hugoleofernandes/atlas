using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using System.Text.Json;

namespace Atlas.BuildingBlocks.Application.OutboxMessages;

public sealed class OutboxMessageFactory : IOutboxMessageFactory
{
    private readonly IRequestContext _requestContext;

    public OutboxMessageFactory(IRequestContext requestContext)
        => _requestContext = requestContext;

    public OutboxMessage Create<T>(T payload)
    {
        return new OutboxMessage(
            name: typeof(T).Name,
            type: typeof(T).FullName!,
            payload: JsonSerializer.Serialize(payload),
            tenantId:      _requestContext.TenantId      ?? throw new InvalidOperationException($"TenantId is required in {nameof(IRequestContext)} to create an OutboxMessage."),
            userId:        _requestContext.UserId         ?? throw new InvalidOperationException($"UserId is required in {nameof(IRequestContext)} to create an OutboxMessage."),
            correlationId: _requestContext.CorrelationId  ?? throw new InvalidOperationException($"CorrelationId is required in {nameof(IRequestContext)} to create an OutboxMessage."),
            module: GetModule(typeof(T))
        );
    }

    private static string GetModule(Type type)
        => type.Namespace?.Split('.')[1] ?? "Unknown";
}