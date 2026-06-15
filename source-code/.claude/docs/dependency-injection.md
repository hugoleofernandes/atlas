# Dependency Injection

## Rules

✅ One entry point per module: `Add{Module}ModuleDependencies()` in `Atlas.{Module}.Infrastructure/DI/`
✅ Module-shared services (UoW, seeders, cache, label providers) registered at the entry point
✅ Aggregates split by dedicated method: `Add{Aggregate}Services()` called from the entry point
✅ Outbox target DI co-located with the target handler — `DependencyInjection.cs` in the same folder as the handler
✅ BuildingBlocks registration methods named `Add{Name}Services()`
✅ Cross-cutting API services wrapped in `AddAtlasCoreServices()` in `Atlas.API/DI/`
✅ OutboxPublisher mappings called alongside the module in `Program.cs` — never inside the module DI (project dependency would reverse)
❌ Never register cross-cutting services loose in `Program.cs` — they belong in `AddAtlasCoreServices()`
❌ Never use partial classes for module DI — use separate files per aggregate

## Naming

| What | Pattern | Example |
|---|---|---|
| Module entry point | `Add{Module}ModuleDependencies()` | `AddIdentityModuleDependencies()` |
| OutboxPublisher mappings | `Add{Module}OutboxPublisherMappings()` | `AddIdentityOutboxPublisherMappings()` |
| Aggregate services | `Add{Aggregate}Services()` | `AddTenantAggregateServices()` |
| Outbox target | `Add{EventName}Target()` | `AddSendInvitationEmailTarget()` |
| BuildingBlock | `Add{Name}Services()` | `AddResendEmailServices()` |
| API cross-cutting | `AddAtlasCoreServices()` | (single, in Atlas.API) |

## Module Entry Point Pattern

```csharp
// Atlas.{Module}.Infrastructure/DI/{Module}DependencyInjection.cs
public static IServiceCollection Add{Module}ModuleDependencies(this IServiceCollection services)
{
    // Shared module infrastructure
    services.AddScoped<I{Module}UnitOfWork, {Module}UnitOfWork>();
    services.AddScoped<{Module}ModuleSeeder>();
    services.AddSingleton<ISomeModuleCache, InMemorySomeModuleCache>();
    services.AddScoped<IAuditLabelProvider, {Module}AuditLabelProvider>();
    services.AddScoped<IPermissionLabelProvider, {Module}PermissionLabelProvider>();

    // Aggregates — each file groups: repo + readers + handlers
    services.Add{Aggregate1}Services();
    services.Add{Aggregate2}Services();

    return services;
}
```

## Aggregate Services Pattern

```csharp
// Atlas.{Module}.Infrastructure/DI/Aggregates/{Aggregate}ServicesExtensions.cs
internal static IServiceCollection Add{Aggregate}Services(this IServiceCollection services)
{
    services.AddScoped<I{Aggregate}Repository, {Aggregate}Repository>();
    services.AddScoped<I{List}Reader, {List}Reader>();
    services.AddScoped<I{Create}CommandHandler, {Create}CommandHandler>();
    services.AddScoped<I{List}QueryHandler, {List}QueryHandler>();
    return services;
}
```

## Outbox Target — Co-Located DI

```
Atlas.Outbox.Targets.{Module}/
└── {EventName}/
    ├── {EventName}TargetHandler.cs
    ├── {EventName}CommandHandler.cs
    └── DependencyInjection.cs       ← registers both handler and target
```

The catalog aggregates targets for the module:
```csharp
public static IServiceCollection Add{Module}OutboxTargets(this IServiceCollection services)
{
    services.Add{EventName1}Target();
    services.Add{EventName2}Target();
    return services;
}
```

## Program.cs — Composition Root Shape

```csharp
services.AddAtlasCoreServices();         // request context, pipeline, localizers

// DbContexts (host owns connection strings)
services.AddDbContext<IdentityDbContext>(...);

// Modules
services.AddIdentityModuleDependencies();
services.AddIdentityOutboxPublisherMappings();  // called here — not inside module DI
services.AddStaffModuleDependencies();
services.AddPlatformModuleDependencies();
services.AddOutboxManagementDependencies();
```
