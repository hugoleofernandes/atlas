using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.BuildingBlocks.Outbox.ListPendingMessages;

public interface IListPendingMessagesQueryHandler
    : IQueryHandler<ListPendingMessagesQuery, IReadOnlyList<ListPendingMessagesDto>> { }
