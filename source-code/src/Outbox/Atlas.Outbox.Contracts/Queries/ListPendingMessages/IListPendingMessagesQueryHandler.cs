using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Contracts.Queries.ListPendingMessages;

public interface IListPendingMessagesQueryHandler
    : IQueryHandler<ListPendingMessagesQuery, IReadOnlyList<ListPendingMessagesDto>> { }
