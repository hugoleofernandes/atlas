using Atlas.OutboxWorker.Configuration;
using Atlas.OutboxWorker.Dispatching;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.Options;

namespace Atlas.OutboxWorker.Processing;

internal sealed class OutboxProcessor : IOutboxProcessor
{
    private readonly IEnumerable<IOutboxWorkerRepository> _repositories;
    private readonly IOutboxMessageDispatcher _dispatcher;
    private readonly OutboxWorkerOptions _options;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IEnumerable<IOutboxWorkerRepository> repositories,
        IOutboxMessageDispatcher dispatcher,
        IOptions<OutboxWorkerOptions> options,
        ILogger<OutboxProcessor> logger)
    {
        _repositories = repositories;
        _dispatcher = dispatcher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task ProcessBatchAsync(CancellationToken ct)
    {
        foreach (var repository in _repositories)
            await ProcessRepositoryAsync(repository, ct);
    }

    private async Task ProcessRepositoryAsync(IOutboxWorkerRepository repository, CancellationToken ct)
    {
        // O banco garante exclusividade via FOR UPDATE SKIP LOCKED —
        // mensagens retornadas já estão atomicamente bloqueadas para este batch.
        var messages = await repository.GetPendingBatchAsync(
            _options.BatchSize, _options.LockDuration, ct);

        if (messages.Count == 0)
            return;

        _logger.LogDebug("Processing batch of {Count} messages", messages.Count);

        // Dead-letter imediato para mensagens que excederam o limite de retries
        foreach (var m in messages.Where(m => m.HasExceededRetries(_options.MaxRetries)))
        {
            m.MarkAsDeadLettered();
            _logger.LogError("Dead-lettered message {MessageId} ({Type}) — exceeded {Max} retries at pickup",
                m.Id, m.Name, _options.MaxRetries);
        }

        var eligible = messages.Where(m => !m.IsDeadLettered).ToList();

        if (eligible.Count > 0)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _options.DegreeOfParallelism,
                CancellationToken = ct
            };

            await Parallel.ForEachAsync(eligible, parallelOptions, async (message, token) =>
                await ProcessMessageAsync(message, token));
        }

        await repository.SaveChangesAsync(ct);
    }

    private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken ct)
    {
        try
        {
            await _dispatcher.DispatchAsync(message, ct);
            message.MarkAsProcessed();
            _logger.LogInformation("Processed message {MessageId} ({Type})", message.Id, message.Name);
        }
        catch (Exception ex)
        {
            message.MarkAsFailed(ex.Message);
            _logger.LogWarning(ex, "Failed to process message {MessageId} ({Type}), retry {Retry}/{Max}",
                message.Id, message.Name, message.RetryCount, _options.MaxRetries);

            if (message.HasExceededRetries(_options.MaxRetries))
            {
                message.MarkAsDeadLettered();
                _logger.LogError("Dead-lettered message {MessageId} ({Type}) after {Max} retries",
                    message.Id, message.Name, _options.MaxRetries);
            }
        }
    }
}
