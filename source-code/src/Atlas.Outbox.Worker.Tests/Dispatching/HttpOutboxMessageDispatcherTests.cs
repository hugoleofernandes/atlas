using System.Text.Json;
using Atlas.BuildingBlocks.Application.ApiInvokers;
using Atlas.Outbox.Infrastructure;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.SharedKernel.Application.OutboxMessages;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Atlas.OutboxWorker.Tests.Dispatching;

public sealed class HttpOutboxMessageDispatcherTests
{
    private readonly IIntegrationEventTypeResolver _typeResolver = Substitute.For<IIntegrationEventTypeResolver>();
    private readonly FakeApiInvoker _apiInvoker = new();

    [Fact]
    public async Task DispatchAsync_WhenSubscriptionsAreConfigured_ShouldInvokeEnabledSubscriptionsInOrder()
    {
        var message = CreateMessage();
        _typeResolver.Resolve(message.Type).Returns(typeof(HttpFakeIntegrationEvent));

        var sut = CreateSut(new OutboxWorkerOptions
        {
            Subscriptions =
            {
                [nameof(HttpFakeIntegrationEvent)] =
                [
                    new() { Name = "identity.email", Method = "POST", Url = "https://atlas.test/email", Order = 20 },
                    new() { Name = "staff.create", Method = "POST", Url = "https://atlas.test/staff", Order = 10 },
                    new() { Name = "disabled", Method = "POST", Url = "https://atlas.test/disabled", Order = 5, Enabled = false }
                ]
            }
        });

        var results = await sut.DispatchAsync(message, CancellationToken.None);

        results.Select(r => r.HandlerName).Should().Equal("staff.create", "identity.email");
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        _apiInvoker.Requests.Select(r => r.Name).Should().Equal("staff.create", "identity.email");
    }

    [Fact]
    public async Task DispatchAsync_WhenOneSubscriptionFails_ShouldContinueAndReturnMixedResults()
    {
        var message = CreateMessage();
        _typeResolver.Resolve(message.Type).Returns(typeof(HttpFakeIntegrationEvent));
        _apiInvoker.Failures.Add("staff.create", "503 service unavailable");

        var sut = CreateSut(new OutboxWorkerOptions
        {
            Subscriptions =
            {
                [nameof(HttpFakeIntegrationEvent)] =
                [
                    new() { Name = "staff.create", Method = "POST", Url = "https://atlas.test/staff", Order = 10 },
                    new() { Name = "identity.email", Method = "POST", Url = "https://atlas.test/email", Order = 20 }
                ]
            }
        });

        var results = await sut.DispatchAsync(message, CancellationToken.None);

        results.Should().HaveCount(2);
        results.Should().ContainSingle(r => !r.IsSuccess && r.HandlerName == "staff.create");
        results.Should().ContainSingle(r => r.IsSuccess && r.HandlerName == "identity.email");
    }

    [Fact]
    public async Task DispatchAsync_WhenInvokingSubscription_ShouldPropagateOutboxMetadata()
    {
        var message = CreateMessage();
        _typeResolver.Resolve(message.Type).Returns(typeof(HttpFakeIntegrationEvent));

        var sut = CreateSut(new OutboxWorkerOptions
        {
            Subscriptions =
            {
                [nameof(HttpFakeIntegrationEvent)] =
                [
                    new() { Name = "staff.create", Method = "POST", Url = "https://atlas.test/staff", Order = 10 }
                ]
            }
        });

        await sut.DispatchAsync(message, CancellationToken.None);

        var request = _apiInvoker.Requests.Single();
        request.IdempotencyKey.Should().Be(message.IdempotencyKey);
        request.OutboxMessageId.Should().Be(message.Id);
        request.CorrelationId.Should().Be(message.CorrelationId);
        request.TenantId.Should().Be(message.TenantId);
        request.UserId.Should().Be(message.UserId);
        request.UserEmail.Should().Be(message.UserEmail);
        request.Payload.Should().BeOfType<HttpFakeIntegrationEvent>();
    }

    [Fact]
    public async Task DispatchAsync_WhenNoSubscriptionIsConfigured_ShouldThrow()
    {
        var message = CreateMessage();
        _typeResolver.Resolve(message.Type).Returns(typeof(HttpFakeIntegrationEvent));
        var sut = CreateSut(new OutboxWorkerOptions());

        var act = () => sut.DispatchAsync(message, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No HTTP subscription configured*");
    }

    private HttpOutboxMessageDispatcher CreateSut(OutboxWorkerOptions options)
        => new(_typeResolver, _apiInvoker, Options.Create(options));

    private static OutboxMessage CreateMessage()
    {
        var payload = JsonSerializer.Serialize(new HttpFakeIntegrationEvent("hello"));

        return new OutboxMessage(
            name: nameof(HttpFakeIntegrationEvent),
            type: "HttpFakeIntegrationEvent",
            payload: payload,
            tenantId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            userEmail: "worker@atlas.test",
            correlationId: "corr-123",
            module: "identity");
    }

    private sealed record HttpFakeIntegrationEvent(string Value);

    private sealed class FakeApiInvoker : IApiInvoker
    {
        public List<ApiInvocationRequest> Requests { get; } = [];
        public Dictionary<string, string> Failures { get; } = [];

        public Task<ApiInvocationResult> InvokeAsync(ApiInvocationRequest request, CancellationToken ct)
        {
            Requests.Add(request);

            return Task.FromResult(Failures.TryGetValue(request.Name, out var error)
                ? ApiInvocationResult.Failure(503, error)
                : ApiInvocationResult.Success(204));
        }
    }
}
