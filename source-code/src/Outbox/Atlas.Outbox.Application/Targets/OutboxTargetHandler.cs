using System.Text.Json;
using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Targets;

public abstract class OutboxTargetHandler<TEvent, TCommand>(
    IHandler<TCommand, Unit> handler,
    IHandlerInvoker invoker,
    IIdempotencyContextSetter idempotencyContextSetter
) : ITargetHandler
{
    public abstract string Name { get; }

    protected abstract TCommand MapToCommand(TEvent @event);

    public async Task<HandlerInvocationResult> ExecuteAsync(ListPendingMessagesDto message, CancellationToken ct)
    {
        var @event =
            JsonSerializer.Deserialize<TEvent>(message.Payload)
            ?? throw new InvalidOperationException($"Failed to deserialize payload for type '{message.Type}'.");

        idempotencyContextSetter.Set(message.IdempotencyKey, Name);

        try
        {
            var result = await invoker.InvokeAsync(handler, MapToCommand(@event), ct);

            return result.IsSuccess
                ? HandlerInvocationResult.Success(Name)
                : HandlerInvocationResult.Failure(Name, result.ErrorDefinition!.FallbackMessage);
        }
        catch (Exception ex)
        {
            return HandlerInvocationResult.Failure(Name, ex);
        }
    }
}
