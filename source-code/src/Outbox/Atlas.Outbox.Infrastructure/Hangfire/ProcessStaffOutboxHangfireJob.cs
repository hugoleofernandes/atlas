using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using Atlas.Outbox.Infrastructure.Configuration;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Infrastructure.Hangfire;

[Queue(HangfireOutboxQueues.Staff)]
public sealed class ProcessStaffOutboxHangfireJob(
    IStaffOutboxProcessingWorkflow workflow,
    IOptions<OutboxWorkerOptions> workerOptions,
    IOptions<HangfireOutboxOptions> hangfireOptions,
    ILogger<ProcessStaffOutboxHangfireJob> logger)
{
    [DisableConcurrentExecution(timeoutInSeconds: 0)]
    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task ExecuteAsync(IJobCancellationToken cancellationToken)
    {
        var worker = workerOptions.Value;
        var hangfire = hangfireOptions.Value;
        var ct = cancellationToken.ShutdownToken;
        var command = new ProcessOutboxCommand(worker.BatchSize, worker.MaxRetries, worker.LockDuration, "staff");
        var startedAt = DateTime.UtcNow;

        using (logger.BeginScope(new Dictionary<string, object> { ["Module"] = "staff" }))
        {
            logger.LogInformation(
                "HangfireOutbox: staff job started (window={Window}s, poll={Poll}s)",
                hangfire.ProcessingWindow.TotalSeconds,
                worker.PollInterval.TotalSeconds);

            do
            {
                ct.ThrowIfCancellationRequested();
                await workflow.RunAsync(command, ct);

                if (DateTime.UtcNow - startedAt >= hangfire.ProcessingWindow)
                    break;

                await Task.Delay(worker.PollInterval, ct);
            }
            while (!ct.IsCancellationRequested);

            logger.LogInformation(
                "HangfireOutbox: staff job completed in {Elapsed:F1}s",
                (DateTime.UtcNow - startedAt).TotalSeconds);
        }
    }
}
