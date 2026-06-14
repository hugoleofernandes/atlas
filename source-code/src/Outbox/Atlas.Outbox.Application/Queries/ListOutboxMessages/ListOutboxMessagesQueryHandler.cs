using Atlas.Outbox.Domain.Exceptions;

namespace Atlas.Outbox.Application.Queries.ListOutboxMessages;

public sealed class ListOutboxMessagesQueryHandler(IListOutboxMessagesReader reader)
    : IIdentityListOutboxMessagesQueryHandler,
      IStaffListOutboxMessagesQueryHandler
{
    private static readonly TimeSpan DefaultWindow = TimeSpan.FromHours(24);
    private static readonly TimeSpan MaxWindow = TimeSpan.FromDays(7);

    public Task<IReadOnlyList<OutboxMessageRow>> ExecuteAsync(
        ListOutboxMessagesQuery query,
        CancellationToken ct)
    {
        var to = query.To ?? DateTime.UtcNow;
        var from = query.From ?? to - DefaultWindow;

        if (from > to)
            throw new OutboxQueryWindowInvalidException(from, to);

        if (to - from > MaxWindow)
            throw new OutboxQueryWindowTooLargeException(MaxWindow);

        return reader.ReadAsync(from, to, ct);
    }
}
