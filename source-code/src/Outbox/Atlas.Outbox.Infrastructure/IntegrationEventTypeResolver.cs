using System.Reflection;
using Atlas.Outbox.Application.Queries.ResolveOutboxTargets;

namespace Atlas.Outbox.Infrastructure;

internal sealed class IntegrationEventTypeResolver : IIntegrationEventTypeResolver
{
    private readonly Dictionary<string, Type> _typeMap;

    public IntegrationEventTypeResolver(IEnumerable<Assembly> assemblies)
    {
        _typeMap = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToDictionary(t => t.FullName!, t => t, StringComparer.Ordinal);
    }

    public Type? Resolve(string typeName)
        => _typeMap.GetValueOrDefault(typeName);
}
