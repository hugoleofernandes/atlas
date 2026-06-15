using Atlas.Outbox.Application.Queries.ListOutboxMessages;
using Atlas.SharedKernel.Modules;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Outbox.Infrastructure.Readers.ListOutboxMessages;

/// <summary>
/// Dapper reader for the outbox investigation screen. Returns one flat row per attempt
/// (no chain grouping — the frontend groups by IdempotencyKey) with the handler
/// executions of each attempt embedded.
///
/// Two queries — messages in the window, then executions for the returned ids —
/// to avoid row multiplication on the 1:N side. The schema string is a compile-time
/// constant per module subclass, safe to interpolate.
/// </summary>
public abstract class ListOutboxMessagesReader : IListOutboxMessagesReader
{
    private readonly DbContext _db;
    private readonly string _schema;
    private readonly AtlasModule _module;

    protected ListOutboxMessagesReader(DbContext db, string schema, AtlasModule module)
    {
        _db = db;
        _schema = schema;
        _module = module;
    }

    private string MessagesSql =>
        $"""
        SELECT
            id,
            idempotency_key          AS IdempotencyKey,
            parent_outbox_message_id AS ParentOutboxMessageId,
            attempt_number           AS AttemptNumber,
            name,
            occurred_on              AS OccurredOn,
            CASE
                WHEN processed_on     IS NOT NULL THEN 'Processed'
                WHEN dead_lettered_on IS NOT NULL THEN 'DeadLettered'
                WHEN failed_at        IS NOT NULL THEN 'Failed'
                ELSE 'Pending'
            END                      AS Status,
            origin,
            error,
            processed_on             AS ProcessedOn,
            failed_at                AS FailedAt,
            dead_lettered_on         AS DeadLetteredOn,
            tenant_id                AS TenantId,
            user_email               AS UserEmail,
            correlation_id           AS CorrelationId,
            resubmitted_by_email     AS ResubmittedByEmail,
            EXISTS (
                SELECT 1
                FROM   {_schema}.outbox_messages c
                WHERE  c.parent_outbox_message_id = {_schema}.outbox_messages.id
            )                        AS HasReplayChild
        FROM   {_schema}.outbox_messages
        WHERE  occurred_on >= @From AND occurred_on <= @To
        ORDER  BY occurred_on DESC
        """;

    private string ExecutionsSql =>
        $"""
        SELECT
            outbox_message_id AS OutboxMessageId,
            handler_name      AS HandlerName,
            status,
            error_message     AS ErrorMessage,
            attempted_at      AS AttemptedAt
        FROM   {_schema}.outbox_handler_executions
        WHERE  outbox_message_id = ANY(@MessageIds)
        ORDER  BY attempted_at
        """;

    private sealed record MessageRow(
        Guid Id,
        Guid IdempotencyKey,
        Guid? ParentOutboxMessageId,
        int AttemptNumber,
        string Name,
        DateTime OccurredOn,
        string Status,
        string Origin,
        string? Error,
        DateTime? ProcessedOn,
        DateTime? FailedAt,
        DateTime? DeadLetteredOn,
        Guid TenantId,
        string? UserEmail,
        string CorrelationId,
        string? ResubmittedByEmail,
        bool HasReplayChild
    );

    private sealed record ExecutionRow(
        Guid OutboxMessageId,
        string HandlerName,
        string Status,
        string? ErrorMessage,
        DateTime AttemptedAt
    );

    public async Task<IReadOnlyList<OutboxMessageRow>> ReadAsync(
        DateTime from,
        DateTime to,
        CancellationToken ct
    )
    {
        var conn = _db.Database.GetDbConnection();

        var messages = (
            await conn.QueryAsync<MessageRow>(
                new CommandDefinition(MessagesSql, new { From = from, To = to }, cancellationToken: ct)
            )
        ).ToList();

        if (messages.Count == 0)
            return [];

        var executions = await conn.QueryAsync<ExecutionRow>(
            new CommandDefinition(
                ExecutionsSql,
                new { MessageIds = messages.Select(m => m.Id).ToArray() },
                cancellationToken: ct
            )
        );

        var lookup = executions.ToLookup(e => e.OutboxMessageId);

        return messages
            .Select(m =>
            {
                var details = lookup[m.Id]
                    .Select(e => new OutboxHandlerExecutionDetail(
                        HandlerName: e.HandlerName,
                        Status: e.Status,
                        ErrorMessage: e.ErrorMessage,
                        AttemptedAt: e.AttemptedAt
                    ))
                    .ToList();

                return new OutboxMessageRow(
                    Id: m.Id,
                    ModuleId: _module.Id,
                    ModuleName: _module.Name,
                    IdempotencyKey: m.IdempotencyKey,
                    ParentOutboxMessageId: m.ParentOutboxMessageId,
                    AttemptNumber: m.AttemptNumber,
                    Name: m.Name,
                    OccurredOn: m.OccurredOn,
                    Status: m.Status,
                    Origin: m.Origin,
                    Error: m.Error,
                    ProcessedOn: m.ProcessedOn,
                    FailedAt: m.FailedAt,
                    DeadLetteredOn: m.DeadLetteredOn,
                    TenantId: m.TenantId,
                    UserEmail: m.UserEmail,
                    CorrelationId: m.CorrelationId,
                    ResubmittedByEmail: m.ResubmittedByEmail,
                    HasReplayChild: m.HasReplayChild,
                    ExecutionCount: details.Count,
                    Executions: details
                );
            })
            .ToList()
            .AsReadOnly();
    }
}
