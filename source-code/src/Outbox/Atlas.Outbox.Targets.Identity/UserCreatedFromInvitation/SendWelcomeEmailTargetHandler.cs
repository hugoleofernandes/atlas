using System.Text.Json;
using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.Identity.Contracts.Commands.SendWelcomeEmail;
using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Domain.Targets.Names;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Targets.Identity.UserCreatedFromInvitation;

public sealed class SendWelcomeEmailTargetHandler(
    ISendWelcomeEmailCommandHandler handler,
    IHandlerInvoker invoker,
    IIdempotencyContextSetter idempotencyContextSetter
) : ITargetHandler
{
    public string Name => IdentityTargetNames.IdentitySendWelcomeEmail;

    public async Task<HandlerInvocationResult> ExecuteAsync(ListPendingMessagesDto message, CancellationToken ct)
    {
        var @event =
            JsonSerializer.Deserialize<UserCreatedFromInvitationIntegrationEvent>(message.Payload)
            ?? throw new InvalidOperationException($"Failed to deserialize payload for type '{message.Type}'.");

        var command = new SendWelcomeEmailCommand(@event.TenantId, @event.UserId, @event.Email);

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
