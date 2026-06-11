using Atlas.Outbox.Application.Commands.ResubmitDeadLetter;
using Atlas.Outbox.Domain.Exceptions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Atlas.Outbox.Tests.Commands;

public sealed class ResubmitDeadLetterCommandHandlerTests
{
    private readonly IOutboxWorkerRepository _repository = Substitute.For<IOutboxWorkerRepository>();
    private readonly IRequestContext _requestContext = Substitute.For<IRequestContext>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ResubmitDeadLetterCommandHandler CreateHandler()
        => new(_repository, _requestContext, _unitOfWork);

    private static OutboxMessage CreateDeadLettered()
    {
        var msg = new OutboxMessage(
            name: "test.event",
            type: "Atlas.Tests.TestEvent",
            payload: """{"v":1}""",
            tenantId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            userEmail: "actor@example.com",
            correlationId: "corr-1",
            module: "identity");
        msg.MarkAsDeadLettered();
        return msg;
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotFound_WhenMessageDoesNotExist()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).ReturnsNull();
        var handler = CreateHandler();

        var act = () => handler.ExecuteAsync(new ResubmitDeadLetterCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<OutboxMessageNotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsConflict_WhenAlreadyHasChild()
    {
        var dead = CreateDeadLettered();
        _repository.GetByIdAsync(dead.Id, Arg.Any<CancellationToken>()).Returns(dead);
        _repository.HasChildAsync(dead.Id, Arg.Any<CancellationToken>()).Returns(true);
        var handler = CreateHandler();

        var act = () => handler.ExecuteAsync(new ResubmitDeadLetterCommand(dead.Id), default);

        await act.Should().ThrowAsync<OutboxMessageAlreadyResubmittedException>();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBusinessError_WhenNotDeadLettered()
    {
        var msg = new OutboxMessage(
            name: "test.event", type: "T", payload: "{}", tenantId: Guid.NewGuid(),
            userId: Guid.NewGuid(), userEmail: null, correlationId: "c", module: "identity");
        _repository.GetByIdAsync(msg.Id, Arg.Any<CancellationToken>()).Returns(msg);
        _repository.HasChildAsync(msg.Id, Arg.Any<CancellationToken>()).Returns(false);
        var handler = CreateHandler();

        var act = () => handler.ExecuteAsync(new ResubmitDeadLetterCommand(msg.Id), default);

        await act.Should().ThrowAsync<OutboxMessageNotDeadLetteredException>();
    }

    [Fact]
    public async Task ExecuteAsync_HappyPath_AddsReplayRow()
    {
        var dead = CreateDeadLettered();
        var operatorId = Guid.NewGuid();
        const string operatorEmail = "admin@example.com";

        _repository.GetByIdAsync(dead.Id, Arg.Any<CancellationToken>()).Returns(dead);
        _repository.HasChildAsync(dead.Id, Arg.Any<CancellationToken>()).Returns(false);
        _requestContext.UserId.Returns(operatorId);
        _requestContext.UserEmail.Returns(operatorEmail);

        OutboxMessage? captured = null;
        await _repository.AddRetryAsync(
            Arg.Do<OutboxMessage>(m => captured = m),
            Arg.Any<CancellationToken>());

        var handler = CreateHandler();

        var output = await handler.ExecuteAsync(new ResubmitDeadLetterCommand(dead.Id), default);

        captured.Should().NotBeNull();
        output.NewMessageId.Should().Be(captured!.Id);
        captured.AttemptNumber.Should().Be(1);
        captured.Origin.Should().Be(OutboxMessageOrigin.ManualResubmit);
        captured.ResubmittedByUserId.Should().Be(operatorId);
        captured.ResubmittedByEmail.Should().Be(operatorEmail);
        captured.ParentOutboxMessageId.Should().Be(dead.Id);
    }
}
