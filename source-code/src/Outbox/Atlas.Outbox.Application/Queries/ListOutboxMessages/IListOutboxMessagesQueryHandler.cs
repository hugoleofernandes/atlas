using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Application.Queries.ListOutboxMessages;

public interface IListOutboxMessagesQueryHandler
    : IQueryHandler<ListOutboxMessagesQuery, IReadOnlyList<OutboxMessageRow>> { }
