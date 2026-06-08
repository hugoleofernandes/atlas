using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.Outbox.Application.Commands.ProcessOutbox;
using Atlas.Outbox.Application.Commands.ProcessOutboxTargets;
using Atlas.Outbox.Application.Commands.UpdateOutboxMessageStatus;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;
using Atlas.Outbox.Domain.Targets;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.OutboxMessages;
using Microsoft.Extensions.Logging;

namespace Atlas.Outbox.Application.Workflows.OutboxProcessing;

/// <summary>
/// Orchestrates outbox processing as an explicit, readable sequence of steps.
/// Calls application handlers via IHandlerInvoker - same pattern as all other modules.
///
/// Current steps:
///   Step 1 - Fetch pending messages via query handler.
///   Step 2 - Resolve target mappings for each event.
///   Step 3 - Dispatch to each resolved target.
///   Step 4 - Update outbox message status.
/// </summary>
public sealed class OutboxProcessingWorkflow(
    IListPendingMessagesQueryHandler getPendingMessages,
    IResolveOutboxTargetsQueryHandler resolveOutboxTargets,
    IProcessOutboxTargetsCommandHandler processOutboxTargets,
    IUpdateOutboxMessageStatusCommandHandler updateOutboxMessageStatus,
    IHandlerInvoker invoker,
    ILogger<OutboxProcessingWorkflow> logger
) : IIdentityOutboxProcessingWorkflow, IStaffOutboxProcessingWorkflow
{
    public async Task RunAsync(ProcessOutboxCommand command, CancellationToken ct)
    {
        // Step 1 - Fetch pending messages via query handler.
        var getPendingMessagesQuery = new ListPendingMessagesQuery(command.BatchSize, command.LockDuration);
        var getPendingMessagesResult = await invoker.InvokeAsync(getPendingMessages, getPendingMessagesQuery, ct);

        if (!getPendingMessagesResult.IsSuccess || getPendingMessagesResult.Value is null)
            return;

        var messages = getPendingMessagesResult.Value;

        logger.LogInformation("OutboxWorkflow: {Count} message(s) locked for processing", messages.Count);

        foreach (var message in messages)
        {
            LogLockedMessage(message);
            IReadOnlyList<HandlerInvocationResult> targetResults;

            // Step 2 - Resolve target mappings for each event.
            var resolveTargetsQuery = new ResolveOutboxTargetsQuery(message);
            var resolveTargetsResult = await invoker.InvokeAsync(resolveOutboxTargets, resolveTargetsQuery, ct);

            if (resolveTargetsResult.IsSuccess && resolveTargetsResult.Value is not null)
            {
                var targets = resolveTargetsResult.Value;
                LogResolvedTargets(message, targets);

                if (targets.Count == 0)
                {
                    logger.LogInformation(
                        "OutboxWorkflow: event={EventName} has no registered targets yet. Skipping this cycle.",
                        message.Name
                    );
                    continue;
                }

                // Step 3 - Dispatch to each resolved target.
                var processTargetsCommand = new ProcessOutboxTargetsCommand(message, targets);
                var processTargetsResult = await invoker.InvokeAsync(processOutboxTargets, processTargetsCommand, ct);

                if (processTargetsResult.IsSuccess && processTargetsResult.Value is not null)
                {
                    targetResults = processTargetsResult.Value;
                }
                else
                {
                    var errorMessage =
                        processTargetsResult.ErrorDefinition?.FallbackMessage ?? "Failed to process outbox targets.";

                    targetResults = CreateDispatcherFailureResults(errorMessage);
                }
            }
            else
            {
                targetResults = CreateDispatcherFailureResults(
                    resolveTargetsResult.ErrorDefinition?.FallbackMessage ?? "Failed to resolve outbox targets."
                );
            }

            LogTargetProcessingSummary(message, targetResults);

            // Step 4 - Update outbox message status.
            var updateOutboxMessageStatusCommand = new UpdateOutboxMessageStatusCommand(
                message,
                targetResults,
                command.MaxRetries
            );

            var updateOutboxMessageStatusResult = await invoker.InvokeAsync(
                updateOutboxMessageStatus,
                updateOutboxMessageStatusCommand,
                ct
            );

            LogFinalizationResult(message, updateOutboxMessageStatusResult);
        }
    }

    private static IReadOnlyList<HandlerInvocationResult> CreateDispatcherFailureResults(string errorMessage) =>
        [HandlerInvocationResult.Failure("Dispatcher", errorMessage)];

    private void LogLockedMessage(ListPendingMessagesDto message)
    {
        var now = DateTime.UtcNow;

        logger.LogInformation(
            "OutboxWorkflow: [{Attempt}] {Name} id={Id} tenant={TenantId} lockedUntil={LockedUntil:o} utcNow={UtcNow:o}",
            message.AttemptNumber,
            message.Name,
            message.Id,
            message.TenantId,
            message.LockedUntil,
            now
        );
    }

    private void LogResolvedTargets(ListPendingMessagesDto message, IReadOnlyList<TargetMapping> targets)
    {
        logger.LogInformation(
            "OutboxWorkflow: event={EventName} resolved {TargetCount} target(s): {Targets}",
            message.Name,
            targets.Count,
            string.Join(", ", targets.Select(target => $"{target.Mode}:{target.Name}"))
        );
    }

    private void LogTargetProcessingSummary(
        ListPendingMessagesDto message,
        IReadOnlyList<HandlerInvocationResult> targetResults
    )
    {
        var failures = targetResults.Where(result => !result.IsSuccess).ToList();

        logger.LogInformation(
            "OutboxWorkflow: event={EventName} executed {TargetCount} target(s) with {FailureCount} failure(s): {Results}",
            message.Name,
            targetResults.Count,
            failures.Count,
            string.Join(
                ", ",
                targetResults.Select(result => $"{result.HandlerName}={(result.IsSuccess ? "Success" : "Failure")}")
            )
        );
    }

    private void LogFinalizationResult(
        ListPendingMessagesDto message,
        Result<UpdateOutboxMessageStatusOutput> updateResult
    )
    {
        if (!updateResult.IsSuccess || updateResult.Value is null)
        {
            logger.LogWarning(
                "OutboxWorkflow: failed to persist final status for message {MessageId}. Error={Error}",
                message.Id,
                updateResult.ErrorDefinition?.FallbackMessage ?? "Unknown"
            );
            return;
        }

        var summary = updateResult.Value;

        logger.LogInformation(
            "OutboxWorkflow: message {MessageId} finalized as {Status} ({ExecutionCount} execution(s), {FailureCount} failure(s))",
            message.Id,
            summary.Status,
            summary.ExecutionCount,
            summary.FailureCount
        );
    }
}
