using Atlas.SharedKernel.Application.Handlers;

namespace Atlas.Outbox.Application.Queries.ListDeadLetters;

public interface IListDeadLettersQueryHandler
    : IQueryHandler<ListDeadLettersQuery, IReadOnlyList<DeadLetterSummary>> { }
