using Atlas.Outbox.Contracts;
using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Application.Queries.GetPendingMessages;

public interface IGetPendingMessagesQueryHandler
    : IQueryHandler<GetPendingMessagesQuery, IReadOnlyList<OutboxMessageDto>>
{
}
