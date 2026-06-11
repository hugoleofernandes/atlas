using System.Net;
using Atlas.BuildingBlocks.Application.ApiInvokers;
using Atlas.SharedKernel.Application;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Atlas.Outbox.Tests.Dispatching;

public sealed class ApiInvokerTests
{
    [Fact]
    public async Task InvokeAsync_WhenResponseIsSuccess_ShouldReturnSuccessAndSendHeaders()
    {
        var handler = new CapturingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
        var invoker = CreateInvoker(handler, "secret");
        var request = CreateRequest();

        var result = await invoker.InvokeAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(204);
        handler.LastRequest!.Headers.GetValues(InternalApiHeaders.ApiKey).Single().Should().Be("secret");
        handler.LastRequest.Headers.GetValues(InternalApiHeaders.IdempotencyKey).Single().Should().Be(request.IdempotencyKey.ToString());
        handler.LastRequest.Headers.GetValues(InternalApiHeaders.OutboxSubscription).Single().Should().Be(request.Name);
        handler.LastRequest.Headers.GetValues(InternalApiHeaders.CorrelationId).Single().Should().Be(request.CorrelationId);
        handler.LastRequest.Headers.GetValues(InternalApiHeaders.TenantId).Single().Should().Be(request.TenantId.ToString());
        handler.LastRequest.Headers.GetValues(InternalApiHeaders.UserId).Single().Should().Be(request.UserId.ToString());
        handler.LastRequest.Headers.GetValues(InternalApiHeaders.UserEmail).Single().Should().Be(request.UserEmail);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseFails_ShouldReturnFailureWithResponseBody()
    {
        var handler = new CapturingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent("service unavailable")
        });
        var invoker = CreateInvoker(handler, "secret");

        var result = await invoker.InvokeAsync(CreateRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(503);
        result.ErrorMessage.Should().Be("service unavailable");
    }

    private static ApiInvoker CreateInvoker(CapturingHttpMessageHandler handler, string apiKey)
    {
        var client = new HttpClient(handler);
        var factory = new FakeHttpClientFactory(client);

        return new ApiInvoker(
            factory,
            LoggerFactory.Create(_ => { }),
            Options.Create(new ApiInvokerOptions
            {
                InternalApiKey = apiKey,
                Timeout = TimeSpan.FromSeconds(5)
            }));
    }

    private static ApiInvocationRequest CreateRequest()
        => new(
            Name: "staff.create",
            Method: HttpMethod.Post,
            Url: new Uri("https://atlas.test/internal"),
            Payload: new { Value = "hello" },
            IdempotencyKey: Guid.NewGuid(),
            OutboxMessageId: Guid.NewGuid(),
            CorrelationId: "corr-123",
            TraceParent: "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00",
            TenantId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            UserEmail: "worker@atlas.test");

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public FakeHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public HttpRequestMessage? LastRequest { get; private set; }

        public CapturingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_responseFactory(request));
        }
    }
}
