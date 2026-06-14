using Atlas.Outbox.Application.Queries.ListOutboxMessages;
using Atlas.Outbox.Domain.Exceptions;
using FluentAssertions;
using NSubstitute;

namespace Atlas.Outbox.Tests.Queries;

public sealed class ListOutboxMessagesQueryHandlerTests
{
    private readonly IListOutboxMessagesReader _reader = Substitute.For<IListOutboxMessagesReader>();

    private ListOutboxMessagesQueryHandler CreateHandler() => new(_reader);

    public ListOutboxMessagesQueryHandlerTests()
    {
        _reader
            .ReadAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns([]);
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsToLast24Hours_WhenNoBoundsProvided()
    {
        var handler = CreateHandler();
        var before = DateTime.UtcNow;

        await handler.ExecuteAsync(new ListOutboxMessagesQuery(From: null, To: null), default);

        var after = DateTime.UtcNow;
        await _reader
            .Received(1)
            .ReadAsync(
                Arg.Is<DateTime>(f => f >= before.AddHours(-24) && f <= after.AddHours(-24)),
                Arg.Is<DateTime>(t => t >= before && t <= after),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UsesExplicitBounds_WhenProvided()
    {
        var from = new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc);
        var handler = CreateHandler();

        await handler.ExecuteAsync(new ListOutboxMessagesQuery(from, to), default);

        await _reader.Received(1).ReadAsync(from, to, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenFromIsAfterTo()
    {
        var from = new DateTime(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc);
        var handler = CreateHandler();

        var act = () => handler.ExecuteAsync(new ListOutboxMessagesQuery(from, to), default);

        await act.Should().ThrowAsync<OutboxQueryWindowInvalidException>();
    }

    [Fact]
    public async Task ExecuteAsync_Throws_WhenWindowExceeds7Days()
    {
        var from = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2026, 6, 9, 0, 0, 1, DateTimeKind.Utc);
        var handler = CreateHandler();

        var act = () => handler.ExecuteAsync(new ListOutboxMessagesQuery(from, to), default);

        await act.Should().ThrowAsync<OutboxQueryWindowTooLargeException>();
    }

    [Fact]
    public async Task ExecuteAsync_AllowsWindowOfExactly7Days()
    {
        var from = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddDays(7);
        var handler = CreateHandler();

        var act = () => handler.ExecuteAsync(new ListOutboxMessagesQuery(from, to), default);

        await act.Should().NotThrowAsync();
    }
}
