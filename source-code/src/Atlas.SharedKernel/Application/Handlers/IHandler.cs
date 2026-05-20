namespace Atlas.SharedKernel.Application.Handlers;

/// <summary>
/// Marker interface for any executable unit (command handler, query handler).
/// Use <see cref="IHandlerInvoker"/> to execute with automatic telemetry and logging.
/// </summary>
public interface IHandler<TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct);
}
