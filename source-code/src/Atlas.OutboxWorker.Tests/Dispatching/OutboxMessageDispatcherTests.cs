//using System.Text.Json;
//using Atlas.OutboxWorker.Dispatching;
//using Atlas.SharedKernel.Application.IntegrationEvents;
//using Atlas.SharedKernel.Application.OutboxMessages;
//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging.Abstractions;
//using NSubstitute;

//namespace Atlas.OutboxWorker.Tests.Dispatching;

//public class OutboxMessageDispatcherTests
//{
//    // ============================================================
//    // 1. HANDLER INVOCADO CORRETAMENTE
//    // ============================================================

//    [Fact]
//    public async Task Dispatch_ShouldCallCorrectHandler_WhenTypeIsKnown()
//    {
//        var handler = Substitute.For<IIntegrationEventHandler<FakeIntegrationEvent>>();

//        var sp = BuildServiceProvider(handler);
//        var resolver = BuildResolver(typeof(FakeIntegrationEvent));
//        var dispatcher = new OutboxMessageDispatcher(resolver, sp, NullLogger<OutboxMessageDispatcher>.Instance);

//        var @event = new FakeIntegrationEvent("hello");
//        var message = CreateMessage(typeof(FakeIntegrationEvent).FullName!, @event);

//        await dispatcher.DispatchAsync(message, default);

//        await handler.Received(1)
//            .HandleAsync(Arg.Is<FakeIntegrationEvent>(e => e.Value == "hello"), Arg.Any<CancellationToken>());
//    }

//    // ============================================================
//    // 2. TIPO NÃO ENCONTRADO → EXCEÇÃO
//    // ============================================================

//    [Fact]
//    public async Task Dispatch_ShouldThrow_WhenTypeNotFound()
//    {
//        var resolver = Substitute.For<IIntegrationEventTypeResolver>();
//        resolver.Resolve(Arg.Any<string>()).Returns((Type?)null);

//        var sp = new ServiceCollection().BuildServiceProvider();
//        var dispatcher = new OutboxMessageDispatcher(resolver, sp, NullLogger<OutboxMessageDispatcher>.Instance);
//        var message = CreateMessage("Unknown.Type", new FakeIntegrationEvent("x"));

//        var act = () => dispatcher.DispatchAsync(message, default);

//        await act.Should().ThrowAsync<InvalidOperationException>()
//            .WithMessage("*Unknown.Type*");
//    }

//    // ============================================================
//    // 3. DESSERIALIZAÇÃO CORRETA
//    // ============================================================

//    [Fact]
//    public async Task Dispatch_ShouldDeserializePayloadCorrectly()
//    {
//        FakeIntegrationEvent? received = null;
//        var handler = Substitute.For<IIntegrationEventHandler<FakeIntegrationEvent>>();
//        handler.HandleAsync(Arg.Do<FakeIntegrationEvent>(e => received = e), Arg.Any<CancellationToken>())
//            .Returns(Task.CompletedTask);

//        var sp = BuildServiceProvider(handler);
//        var resolver = BuildResolver(typeof(FakeIntegrationEvent));
//        var dispatcher = new OutboxMessageDispatcher(resolver, sp, NullLogger<OutboxMessageDispatcher>.Instance);

//        var message = CreateMessage(typeof(FakeIntegrationEvent).FullName!, new FakeIntegrationEvent("deserialized-value"));

//        await dispatcher.DispatchAsync(message, default);

//        received.Should().NotBeNull();
//        received!.Value.Should().Be("deserialized-value");
//    }

//    // ============================================================
//    // 4. HANDLER NÃO REGISTRADO → EXCEÇÃO
//    // ============================================================

//    [Fact]
//    public async Task Dispatch_ShouldThrow_WhenHandlerNotRegistered()
//    {
//        var sp = new ServiceCollection().BuildServiceProvider(); // nenhum handler
//        var resolver = BuildResolver(typeof(FakeIntegrationEvent));
//        var dispatcher = new OutboxMessageDispatcher(resolver, sp, NullLogger<OutboxMessageDispatcher>.Instance);

//        var message = CreateMessage(typeof(FakeIntegrationEvent).FullName!, new FakeIntegrationEvent("x"));

//        var act = () => dispatcher.DispatchAsync(message, default);

//        await act.Should().ThrowAsync<InvalidOperationException>()
//            .WithMessage("*FakeIntegrationEvent*");
//    }

//    // ============================================================
//    // HELPERS
//    // ============================================================

//    private static IServiceProvider BuildServiceProvider(IIntegrationEventHandler<FakeIntegrationEvent> handler)
//    {
//        var services = new ServiceCollection();
//        services.AddSingleton(handler);
//        return services.BuildServiceProvider();
//    }

//    private static IIntegrationEventTypeResolver BuildResolver(Type type)
//    {
//        var resolver = Substitute.For<IIntegrationEventTypeResolver>();
//        resolver.Resolve(type.FullName!).Returns(type);
//        return resolver;
//    }

//    private static OutboxMessage CreateMessage<T>(string typeName, T @event) =>
//        new("fake.event",
//            typeName,
//            JsonSerializer.Serialize(@event),
//            tenantId: null,
//            userId: null,
//            correlationId: null,
//            module: "tests");
//}

//public sealed record FakeIntegrationEvent(string Value);
