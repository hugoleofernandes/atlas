//using System.Text.Json;

//namespace Atlas.Outbox.Targets.Staff.Identity.UserCreatedFromInvitation;

//public sealed class CreateStaffMemberFromInvitationTargetHandler(
//    ICreateStaffMemberFromInvitationCommandHandler handler,
//    IHandlerInvoker invoker,
//    IIdempotencyContextSetter idempotencyContextSetter
//) : IDirectTargetHandler
//{
//    public string Name => UserCreatedFromInvitationDirectTargetCatalog.StaffCreateMemberFromInvitation;

//    public async Task<HandlerInvocationResult> ExecuteAsync(OutboxMessageDto message, CancellationToken ct)
//    {
//        var @event =
//            JsonSerializer.Deserialize<UserCreatedFromInvitationIntegrationEvent>(message.Payload)
//            ?? throw new InvalidOperationException($"Failed to deserialize payload for type '{message.Type}'.");

//        var command = new CreateStaffMemberFromInvitationCommand(
//            @event.TenantId,
//            @event.UserId,
//            @event.Email,
//            @event.Role
//        );

//        idempotencyContextSetter.Set(message.IdempotencyKey, Name);

//        try
//        {
//            var result = await invoker.InvokeAsync(handler, command, ct);

//            return result.IsSuccess
//                ? HandlerInvocationResult.Success(Name)
//                : HandlerInvocationResult.Failure(Name, result.ErrorDefinition!.FallbackMessage);
//        }
//        catch (Exception ex)
//        {
//            return HandlerInvocationResult.Failure(Name, ex);
//        }
//    }
//}
