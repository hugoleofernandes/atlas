using Atlas.OutboxWorker.Configuration;
using Atlas.OutboxWorker.Processing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.OutboxWorker.Hosting;

internal sealed class OutboxWorkerHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxWorkerOptions _options;
    private readonly ILogger<OutboxWorkerHostedService> _logger;

    public OutboxWorkerHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxWorkerOptions> options,
        ILogger<OutboxWorkerHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxWorker started (PollInterval={Interval}s)", _options.PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                await processor.ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in OutboxWorker processing cycle");
            }

            await Task.Delay(_options.PollInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxWorker stopped");
    }
}
