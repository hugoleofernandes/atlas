using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.Outbox.Application.Workflow;
using Atlas.Outbox.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Worker.Hosting;

internal sealed class OutboxWorkerHostedServiceV2 : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxWorkerOptions _options;
    private readonly ILogger<OutboxWorkerHostedServiceV2> _logger;

    public OutboxWorkerHostedServiceV2(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxWorkerOptions> options,
        ILogger<OutboxWorkerHostedServiceV2> logger
    )
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxWorkerV2 started (PollInterval={Interval}s)", _options.PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();

                var identityWorkflow = scope.ServiceProvider.GetRequiredService<IIdentityOutboxProcessingWorkflow>();

                var staffWorkflow = scope.ServiceProvider.GetRequiredService<IStaffOutboxProcessingWorkflow>();

                var command = new ProcessOutboxCommand(_options.BatchSize, _options.MaxRetries, _options.LockDuration);

                await identityWorkflow.RunAsync(command, stoppingToken);
                //await staffWorkflow.RunAsync(command, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in OutboxWorkerV2 processing cycle");
            }

            await Task.Delay(_options.PollInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxWorkerV2 stopped");
    }
}
