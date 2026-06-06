using System.Text.Json;
using Atlas.BuildingBlocks.Application.InternalApiInvokers;
using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.Outbox.Domain.Targets;
using Atlas.SharedKernel.Application.OutboxMessages;

namespace Atlas.Outbox.Application.Commands.ProcessOutboxTargets;

public sealed class HttpTargetExecutor(IInternalApiInvoker invoker) : IOutboxTargetExecutor
{
    public TargetMode Mode => TargetMode.Http;

    public async Task<HandlerInvocationResult> ExecuteAsync(
        TargetMapping target,
        ListPendingMessagesDto message,
        CancellationToken ct
    )
    {
        if (string.IsNullOrWhiteSpace(target.Url))
        {
            return HandlerInvocationResult.Failure(target.Name, $"No URL configured for HTTP target '{target.Name}'.");
        }

        var payload = JsonSerializer.Deserialize<JsonElement>(message.Payload);
        var method = ParseHttpMethod(target.Method);
        var request = new InternalApiInvocationRequest(
            target.Name,
            method,
            new Uri(target.Url, UriKind.Absolute),
            payload,
            message.CorrelationId,
            message.TraceParent
        );

        var result = await invoker.InvokeAsync<object>(request, ct);

        return result.IsSuccess
            ? HandlerInvocationResult.Success(target.Name)
            : HandlerInvocationResult.Failure(target.Name, result.ErrorMessage ?? "HTTP target execution failed.");
    }

    private static HttpMethod ParseHttpMethod(string? method) =>
        string.IsNullOrWhiteSpace(method) ? HttpMethod.Post : new HttpMethod(method);
}
