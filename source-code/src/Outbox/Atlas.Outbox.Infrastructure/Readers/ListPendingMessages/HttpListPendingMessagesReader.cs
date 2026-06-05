using Atlas.BuildingBlocks.Application.InternalApiInvokers;
using Atlas.Outbox.Contracts.Queries.ListPendingMessages;
using Atlas.Outbox.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Infrastructure.Readers.ListPendingMessages;

/// <summary>
/// HTTP strategy for fetching pending outbox messages.
/// Calls the module's InternalApi endpoint instead of reading the DB directly.
/// Used when the outbox worker runs in a separate network zone from the DB.
///
/// Configured via OutboxWorkerOptions.PendingMessagesUrls:
///   "identity": "https://api/internal/identity/outbox/pending-messages"
/// </summary>
public sealed class HttpListPendingMessagesReader : IListPendingMessagesReader
{
    private readonly IInternalApiInvoker _internalApiInvoker;
    private readonly string _url;

    public HttpListPendingMessagesReader(
        IInternalApiInvoker internalApiInvoker,
        IOptions<OutboxWorkerOptions> options,
        string moduleKey
    )
    {
        _internalApiInvoker = internalApiInvoker;

        if (!options.Value.PendingMessagesUrls.TryGetValue(moduleKey, out var url) || string.IsNullOrWhiteSpace(url))
            throw new InvalidOperationException(
                $"OutboxWorker.PendingMessagesUrls[\"{moduleKey}\"] is not configured."
            );

        _url = url;
    }

    public async Task<IReadOnlyList<ListPendingMessagesDto>> ReadAsync(
        int batchSize,
        TimeSpan lockDuration,
        CancellationToken ct
    )
    {
        var url = BuildUrl(batchSize, lockDuration);

        var request = new InternalApiInvocationRequest(
            Name: "outbox.pending-messages",
            Method: HttpMethod.Get,
            Url: url
        );

        var result = await _internalApiInvoker.InvokeAsync<List<ListPendingMessagesDto>>(request, ct);

        if (!result.IsSuccess)
            throw new InvalidOperationException(
                result.ErrorMessage ?? "Failed to fetch pending outbox messages from internal API."
            );

        return result.Value ?? [];
    }

    private Uri BuildUrl(int batchSize, TimeSpan lockDuration)
    {
        var separator = _url.Contains('?') ? "&" : "?";
        var lockDurationSeconds = (int)Math.Ceiling(lockDuration.TotalSeconds);

        return new Uri(
            $"{_url}{separator}batchSize={batchSize}&lockDurationSeconds={lockDurationSeconds}",
            UriKind.Absolute
        );
    }
}
