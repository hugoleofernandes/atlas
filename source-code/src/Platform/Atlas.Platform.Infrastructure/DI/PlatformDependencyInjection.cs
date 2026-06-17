using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Platform.Application.Abstractions;
using Atlas.Platform.Application.Queries.Audit.ListEntries;
using Atlas.Platform.Application.Queries.EntityTypes.Lookup;
using Atlas.Platform.Application.Queries.Geography;
using Atlas.Platform.Application.Queries.Geography.GetCitiesByState;
using Atlas.Platform.Application.Queries.Geography.GetStatesByCountry;
using Atlas.Platform.Application.Queries.Tenants.GetTenantByName;
using Atlas.Platform.Application.Queries.Tenants.GetTenantsByIds;
using Atlas.Platform.Infrastructure.Labels;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.Platform.Infrastructure.Readers.Audit.ListEntries;
using Atlas.Platform.Infrastructure.Readers.EntityTypes.Lookup;
using Atlas.Platform.Infrastructure.Readers.Geography;
using Atlas.Platform.Infrastructure.Readers.Tenants.GetTenantByName;
using Atlas.Platform.Infrastructure.Readers.Tenants.GetTenantsByIds;
using Atlas.Platform.Infrastructure.Seeders;
using Atlas.SharedKernel.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Platform.Infrastructure.DI;

public static class PlatformDependencyInjection
{
    public static IServiceCollection AddPlatformModuleDependencies(this IServiceCollection services)
    {
        // GENERAL
        services.AddSingleton<IEntityTypeCatalogCache, InMemoryEntityTypeCatalogCache>();
        services.AddSingleton<IGeographyCache, InMemoryGeographyCache>();
        services.AddScoped<IPlatformUnitOfWork, PlatformUnitOfWork>();
        services.AddScoped<PlatformModuleSeeder>();
        services.AddScoped<IAuditLabelProvider, PlatformAuditLabelProvider>();
        services.AddScoped<IPermissionLabelProvider, PlatformPermissionLabelProvider>();

        // READERS
        services.AddScoped<ILookupEntityTypesReader, LookupEntityTypesReader>();
        services.AddScoped<IGeographyReader, GeographyReader>();
        services.AddScoped<IGetTenantByNameReader, GetTenantByNameReader>();
        services.AddScoped<IGetTenantsByIdsReader, GetTenantsByIdsReader>();

        // Audit reader registered as concrete type to avoid DI conflict with
        // other modules that also register IListAuditEntriesReader.
        services.AddScoped<PlatformAuditEntriesReader>();

        // QUERY HANDLERS
        services.AddScoped<ILookupEntityTypesQueryHandler, LookupEntityTypesQueryHandler>();
        services.AddScoped<IGetStatesByCountryQueryHandler, GetStatesByCountryQueryHandler>();
        services.AddScoped<IGetCitiesByStateQueryHandler, GetCitiesByStateQueryHandler>();
        services.AddScoped<IGetTenantByNameQueryHandler, GetTenantByNameQueryHandler>();
        services.AddScoped<IGetTenantsByIdsQueryHandler, GetTenantsByIdsQueryHandler>();

        // Factory lambda wires the Platform-specific reader into the shared audit handler
        // without exposing IListAuditEntriesReader in the root DI container.
        services.AddScoped<IPlatformListAuditEntriesQueryHandler>(sp => new PlatformListAuditEntriesQueryHandler(
            sp.GetRequiredService<PlatformAuditEntriesReader>(),
            sp.GetRequiredService<IRequestContext>()
        ));

        return services;
    }
}
