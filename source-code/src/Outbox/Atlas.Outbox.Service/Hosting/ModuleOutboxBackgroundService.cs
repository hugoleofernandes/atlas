using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using Atlas.Outbox.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Service.Hosting;

internal sealed class ModuleOutboxBackgroundService<TWorkflow>(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxWorkerOptions> workerOptions,
    string module,
    ILogger<ModuleOutboxBackgroundService<TWorkflow>> logger
) : BackgroundService
    where TWorkflow : IOutboxProcessingWorkflow
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var _ = logger.BeginScope(new Dictionary<string, object> { ["Module"] = module });

        var options = workerOptions.Value;

        logger.LogInformation(
            "OutboxService: {Module} loop started (poll={Poll}s)",
            module,
            options.PollInterval.TotalSeconds
        );

        using var timer = new PeriodicTimer(options.PollInterval);

        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var workflow = scope.ServiceProvider.GetRequiredService<TWorkflow>();
                var command = new ProcessOutboxCommand(
                    BatchSize: options.BatchSize,
                    MaxRetries: options.MaxRetries,
                    LockDuration: options.LockDuration,
                    Module: module
                );

                await workflow.RunAsync(command, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxService: {Module} tick failed", module);
            }
        }
    }
}
