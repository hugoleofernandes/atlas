using Atlas.SharedKernel.Application.OutboxMessages;
using System.Text.Json;

namespace Atlas.BuildingBlocks.Application.OutboxMessages;

public sealed class OutboxMessageFactory : IOutboxMessageFactory
{
    public OutboxMessage Create<T>(T payload)
    {
        return new OutboxMessage(
            name: typeof(T).Name,
            type: typeof(T).FullName!,
            payload: JsonSerializer.Serialize(payload),
            tenantId: null,
            userId: null,
            correlationId: null,
            module: GetModule(typeof(T))
        );
    }

    private static string GetModule(Type type)
    {
        return type.Namespace?.Split('.')[1] ?? "Unknown";
    }
}