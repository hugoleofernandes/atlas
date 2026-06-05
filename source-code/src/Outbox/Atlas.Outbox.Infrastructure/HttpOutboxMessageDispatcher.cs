using System.Net.Http;
using System.Text.Json;
using Atlas.BuildingBlocks.Application.ApiInvokers;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Infrastructure;

internal sealed class HttpOutboxMessageDispatcher : IOutboxMessageDispatcher
{
    private readonly IIntegrationEventTypeResolver _typeResolver;
    private readonly IApiInvoker _apiInvoker;
    private readonly OutboxWorkerOptions _options;

    public HttpOutboxMessageDispatcher(
        IIntegrationEventTypeResolver typeResolver,
        IApiInvoker apiInvoker,
        IOptions<OutboxWorkerOptions> options)
    {
        _typeResolver = typeResolver;
        _apiInvoker = apiInvoker;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<HandlerInvocationResult>> DispatchAsync(
        OutboxMessage message,
        CancellationToken ct)
    {
        var eventType = _typeResolver.Resolve(message.Type)
            ?? throw new InvalidOperationException($"Integration event type '{message.Type}' not found.");

        var payload = JsonSerializer.Deserialize(message.Payload, eventType)
            ?? throw new InvalidOperationException($"Failed to deserialize payload for type '{message.Type}'.");

        var subscriptions = ResolveSubscriptions(message, eventType);
        if (subscriptions.Count == 0)
            throw new InvalidOperationException($"No HTTP subscription configured for '{eventType.Name}'.");

        var results = new List<HandlerInvocationResult>(subscriptions.Count);

        foreach (var subscription in subscriptions)
        {
            var request = new ApiInvocationRequest(
                Name: subscription.Name,
                Method: new HttpMethod(subscription.Method),
                Url: new Uri(subscription.Url, UriKind.Absolute),
                Payload: payload,
                IdempotencyKey: message.IdempotencyKey,
                OutboxMessageId: message.Id,
                CorrelationId: message.CorrelationId,
                TraceParent: message.TraceParent,
                TenantId: message.TenantId,
                UserId: message.UserId,
                UserEmail: message.UserEmail);

            var result = await _apiInvoker.InvokeAsync(request, ct);

            results.Add(result.IsSuccess
                ? HandlerInvocationResult.Success(subscription.Name)
                : HandlerInvocationResult.Failure(
                    subscription.Name,
                    result.ErrorMessage ?? $"HTTP subscription failed with status {result.StatusCode}."));
        }

        return results;
    }

    private IReadOnlyList<OutboxSubscriptionOptions> ResolveSubscriptions(OutboxMessage message, Type eventType)
    {
        if (!_options.Subscriptions.TryGetValue(message.Name, out var subscriptions)
            && !_options.Subscriptions.TryGetValue(eventType.Name, out subscriptions)
            && !_options.Subscriptions.TryGetValue(message.Type, out subscriptions))
        {
            return Array.Empty<OutboxSubscriptionOptions>();
        }

        return subscriptions
            .Where(s => s.Enabled)
            .OrderBy(s => s.Order)
            .ThenBy(s => s.Name)
            .ToList();
    }
}
