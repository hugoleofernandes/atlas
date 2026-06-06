using Atlas.BuildingBlocks.Outbox.ListPendingMessages;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Application.Queries.ListPendingMessages;

public interface IListPendingMessagesQueryHandler
    : IQueryHandler<ListPendingMessagesQuery, IReadOnlyList<ListPendingMessagesDto>> { }
