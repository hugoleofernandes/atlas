using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Errors;

namespace Atlas.SharedKernel.Application.Handlers;

/// <summary>
/// Convenience extensions for <see cref="IHandlerInvoker"/>.
/// </summary>
public static class HandlerInvokerExtensions
{
    /// <summary>
    /// Invokes the handler and throws <see cref="HandlerResultException"/> if the result
    /// is a failure — preserving the original <see cref="ErrorDefinition"/> (code, category,
    /// message) so the invoker pipeline above can record it as structured observability data.
    ///
    /// Use this in integration event adapters instead of raw <see cref="IHandlerInvoker.InvokeAsync"/>
    /// to guarantee that domain failures surface as failures in the outbox processing chain
    /// (retry / dead-letter) rather than being silently swallowed.
    /// </summary>
    public static async Task InvokeOrThrowAsync<TInput, TOutput>(
        this IHandlerInvoker      invoker,
        IHandler<TInput, TOutput> handler,
        TInput                    input,
        CancellationToken         ct)
    {
        var result = await invoker.InvokeAsync(handler, input, ct);

        if (!result.IsSuccess)
            throw new HandlerResultException(result.ErrorDefinition!);
    }
}
