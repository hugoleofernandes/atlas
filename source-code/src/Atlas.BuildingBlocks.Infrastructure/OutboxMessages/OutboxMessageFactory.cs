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
            tenantId: _requestContext.TenantId,
            userId: _requestContext.UserId,
            correlationId: _requestContext.CorrelationId,
            module: GetModule(typeof(T))
        );
    }

    private static string GetModule(Type type)
        => type.Namespace?.Split('.')[1] ?? "Unknown";
}