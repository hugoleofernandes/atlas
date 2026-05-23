using Atlas.SharedKernel.Application.Handlers;
using Atlas.Outbox.Application.ProcessOutbox;
using Atlas.Outbox.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Worker.Hosting;

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
        _options      = options.Value;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxWorker started (PollInterval={Interval}s)", _options.PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope   = _scopeFactory.CreateAsyncScope();
                var invoker             = scope.ServiceProvider.GetRequiredService<IHandlerInvoker>();
                var identityHandler     = scope.ServiceProvider.GetRequiredService<IIdentityOutboxCommandHandler>();
                var staffHandler        = scope.ServiceProvider.GetRequiredService<IStaffOutboxCommandHandler>();
                var command             = new ProcessOutboxCommand(_options.BatchSize, _options.MaxRetries, _options.LockDuration);

                await invoker.InvokeAsync(identityHandler, command, stoppingToken);
                //await invoker.InvokeAsync(staffHandler,    command, stoppingToken);
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
