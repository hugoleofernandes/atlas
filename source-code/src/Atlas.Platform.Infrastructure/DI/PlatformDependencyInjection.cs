using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.Platform.Application.Abstractions;
using Atlas.Platform.Application.Queries.Audit.ListEntries;
using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Application.Queries.Tenants.GetTenantByName;
using Atlas.Platform.Domain.Permissions;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.Platform.Infrastructure.Readers.Audit.ListEntries;
using Atlas.Platform.Infrastructure.Readers.EntityTypes.Lookup;
using Atlas.Platform.Infrastructure.Readers.Tenants.GetTenantByName;
using Atlas.Platform.Infrastructure.Seeders;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Domain.Permissions;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Platform.Infrastructure.DI;

public static class PlatformDependencyInjection
{
    public static IServiceCollection AddPlatformModuleDependencies(this IServiceCollection services)
    {
        // GENERAL
        services.AddScoped<IPlatformUnitOfWork, PlatformUnitOfWork>();
        services.AddScoped<IModuleSeeder, PlatformModuleSeeder>();

        // PERMISSIONS
        services.AddSingleton<IModulePermissions, PlatformModulePermissions>();

        // READERS
        services.AddScoped<ILookupEntityTypesReader, LookupEntityTypesReader>();
        services.AddScoped<IGetTenantByNameReader, GetTenantByNameReader>();

        // Audit reader registered as concrete type to avoid DI conflict with
        // other modules that also register IListAuditEntriesReader.
        services.AddScoped<PlatformAuditEntriesReader>();

        // QUERY HANDLERS
        services.AddScoped<ILookupEntityTypesQueryHandler, LookupEntityTypesQueryHandler>();
        services.AddScoped<IGetTenantByNameQueryHandler, GetTenantByNameQueryHandler>();

        // Factory lambda wires the Platform-specific reader into the shared audit handler
        // without exposing IListAuditEntriesReader in the root DI container.
        services.AddScoped<IPlatformListAuditEntriesQueryHandler>(sp =>
            new PlatformListAuditEntriesQueryHandler(
                sp.GetRequiredService<PlatformAuditEntriesReader>(),
                sp.GetRequiredService<IRequestContext>()));

        return services;
    }
}
