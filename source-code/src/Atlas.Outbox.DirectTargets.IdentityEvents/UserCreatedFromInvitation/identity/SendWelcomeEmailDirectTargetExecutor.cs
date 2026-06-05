using System.Text.Json;
using Atlas.Identity.Application.Commands.SendWelcomeEmail;
using Atlas.Integration.Contracts.Tenants;
using Atlas.Outbox.Application.Commands.ProcessOutboxTargets;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Contracts;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.DirectTargets.IdentityEvents.UserCreatedFromInvitation.Identity;

public sealed class SendWelcomeEmailDirectTargetExecutor(
    ISendWelcomeEmailCommandHandler handler,
    IHandlerInvoker invoker,
    IIdempotencyContextSetter idempotencyContextSetter)
    : IOutboxTargetExecutor
{
    public string Name => UserCreatedFromInvitationDirectTargetCatalog.IdentitySendWelcomeEmail;
    public OutboxTargetMode Mode => OutboxTargetMode.Direct;

    public async Task<HandlerInvocationResult> ExecuteAsync(
        OutboxMessageDto message,
        CancellationToken ct)
    {
        var @event = JsonSerializer.Deserialize<UserCreatedFromInvitationIntegrationEvent>(message.Payload)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize payload for type '{message.Type}'.");

        var command = new SendWelcomeEmailCommand(
            @event.TenantId,
            @event.UserId,
            @event.Email);

        idempotencyContextSetter.Set(message.IdempotencyKey, Name);

        try
        {
            var result = await invoker.InvokeAsync(handler, command, ct);

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
