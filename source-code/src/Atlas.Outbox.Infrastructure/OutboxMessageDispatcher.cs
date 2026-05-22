using System.Reflection;
using System.Text.Json;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.IntegrationEvents;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Infrastructure;

/// <summary>
/// Resolves the integration event type from the outbox message, deserializes the payload,
/// and invokes all registered <see cref="IIntegrationEventHandler{TEvent}"/> through
/// <see cref="IHandlerInvoker"/> — which handles telemetry, logging, and idempotency.
///
/// Each handler runs independently: a failure in one never prevents the others from executing.
/// Results are returned as a list — one <see cref="HandlerInvocationResult"/> per handler.
///
/// Trace continuation is handled by <see cref="TracingDispatcherDecorator"/>, which wraps
/// this class and opens the parent span before delegating here.
/// </summary>
internal sealed class OutboxMessageDispatcher : IOutboxMessageDispatcher
{
    private readonly IIntegrationEventTypeResolver _typeResolver;
    private readonly IHandlerInvoker               _handlerInvoker;
    private readonly IIdempotencyContextSetter     _idempotencyContextSetter;
    private readonly IServiceProvider              _serviceProvider;

    // Cached open MethodInfo for IHandlerInvoker.InvokeAsync<TInput, TOutput>.
    // MakeGenericMethod(eventType, typeof(Unit)) is called once per event type.
    private static readonly MethodInfo OpenInvokeMethod =
        typeof(IHandlerInvoker)
            .GetMethod(nameof(IHandlerInvoker.InvokeAsync))!;

    public OutboxMessageDispatcher(
        IIntegrationEventTypeResolver typeResolver,
        IHandlerInvoker               handlerInvoker,
        IIdempotencyContextSetter     idempotencyContextSetter,
        IServiceProvider              serviceProvider)
    {
        _typeResolver             = typeResolver;
        _handlerInvoker           = handlerInvoker;
        _idempotencyContextSetter = idempotencyContextSetter;
        _serviceProvider          = serviceProvider;
    }

    public async Task<IReadOnlyList<HandlerInvocationResult>> DispatchAsync(
        OutboxMessage     message,
        CancellationToken ct)
    {
        // ── Resolve event type and deserialize payload ─────────────────────────
        var eventType = _typeResolver.Resolve(message.Type)
            ?? throw new InvalidOperationException(
                $"Integration event type '{message.Type}' not found.");

        var @event = JsonSerializer.Deserialize(message.Payload, eventType)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize payload for type '{message.Type}'.");

        var handlerType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

        List<object> handlers = _serviceProvider.GetServices(handlerType)
            .Where(h => h is not null)
            .Select(h => h!)
            .ToList();

        if (handlers.Count == 0)
            throw new InvalidOperationException(
                $"No handler registered for '{eventType.Name}'.");
        // ───────────────────────────────────────────────────────────────────────

        // ── Invoke every handler — collect results ─────────────────────────────
        // Idempotency context is set per handler before invoking so IIdempotencyService
        // reads the correct (IdempotencyKey, HandlerName) pair.
        // Each handler runs regardless of what others do; exceptions are caught per handler.
        var invokeMethod = OpenInvokeMethod.MakeGenericMethod(eventType, typeof(Unit));
        var results      = new List<HandlerInvocationResult>(handlers.Count);

        foreach (var handler in handlers)
        {
            var handlerName = handler.GetType().Name;

            _idempotencyContextSetter.Set(message.IdempotencyKey, handlerName);

            try
            {
                var resultTask = (Task<Result<Unit>>)invokeMethod.Invoke(
                    _handlerInvoker, [handler, @event, ct])!;

                var result = await resultTask;

                results.Add(result.IsSuccess
                    ? HandlerInvocationResult.Success(handlerName)
                    : HandlerInvocationResult.Failure(handlerName, result.ErrorDefinition!.FallbackMessage));
            }
            catch (Exception ex)
            {
                // Infrastructure-level exception that escaped the invoker pipeline
                // (already logged and spanned by LoggingDecorator / TelemetryDecorator).
                results.Add(HandlerInvocationResult.Failure(handlerName, ex));
            }
        }
        // ───────────────────────────────────────────────────────────────────────

        return results;
    }
}
