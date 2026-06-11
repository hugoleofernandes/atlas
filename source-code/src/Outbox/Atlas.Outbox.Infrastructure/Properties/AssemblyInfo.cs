using System.Runtime.CompilerServices;

// Test project — grants direct access to internal types (OutboxMessageDispatcher, IIntegrationEventTypeResolver).
[assembly: InternalsVisibleTo("Atlas.Outbox.Tests")]

// Castle DynamicProxy (used by NSubstitute) — required to generate proxies for internal interfaces at runtime.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
