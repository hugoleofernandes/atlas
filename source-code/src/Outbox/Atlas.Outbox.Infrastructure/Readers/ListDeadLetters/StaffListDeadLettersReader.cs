using Atlas.Outbox.Application.Queries.ListDeadLetters;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Outbox.Infrastructure.Readers.ListDeadLetters;

public sealed class StaffListDeadLettersReader(StaffDbContext db) : IListDeadLettersReader
{
    private const string Sql = """
        SELECT
            m.id,
            m.name,
            m.module,
            m.attempt_number    AS AttemptNumber,
            m.dead_lettered_on  AS DeadLetteredOn,
            m.error,
            EXISTS (
                SELECT 1 FROM atlas_staff.outbox_messages c
                WHERE  c.parent_outbox_message_id = m.id
            ) AS WasResubmitted
        FROM   atlas_staff.outbox_messages m
        WHERE  m.dead_lettered_on IS NOT NULL
        ORDER  BY m.dead_lettered_on DESC
        """;

    public async Task<IReadOnlyList<DeadLetterSummary>> ReadAsync(CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<DeadLetterSummary>(
            new CommandDefinition(Sql, cancellationToken: ct));
        return rows.ToList().AsReadOnly();
    }
}
