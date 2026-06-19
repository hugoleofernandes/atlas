using Atlas.BuildingBlocks.Application.HandlerInvokers;
using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Audit.Queries;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Party.Application.Abstractions;
using Atlas.Party.Application.Queries.Audit.ListEntries;
using Atlas.Party.Application.Queries.Lookups;
using Atlas.Party.Application.Queries.Lookups.LookupAddressTypes;
using Atlas.Party.Application.Queries.Lookups.LookupClassificationTypes;
using Atlas.Party.Application.Queries.Lookups.LookupContactTypes;
using Atlas.Party.Application.Queries.Lookups.LookupGenders;
using Atlas.Party.Infrastructure.DI.Aggregates;
using Atlas.Party.Infrastructure.Labels;
using Atlas.Party.Infrastructure.Persistence.DbContexts;
using Atlas.Party.Infrastructure.Readers.Audit.ListEntries;
using Atlas.Party.Infrastructure.Readers.Lookups.LookupAddressTypes;
using Atlas.Party.Infrastructure.Readers.Lookups.LookupClassificationTypes;
using Atlas.Party.Infrastructure.Readers.Lookups.LookupContactTypes;
using Atlas.Party.Infrastructure.Readers.Lookups.LookupGenders;
using Atlas.SharedKernel.Application;
using Atlas.Party.Infrastructure.Seeders;
using Atlas.SharedKernel.Application.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Party.Infrastructure.DI;

public static class PartyDependencyInjection
{
    public static IServiceCollection AddPartyModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IPartyUnitOfWork, PartyUnitOfWork>();
        services.AddScoped<PartyModuleSeeder>();
        services.AddScoped<IHandlerInvoker, HandlerInvoker>(); //todo: move to BuildingBlocks.DI when it exists
        services.AddScoped<IAuditLabelProvider, PartyAuditLabelProvider>();
        services.AddScoped<IPermissionLabelProvider, PartyPermissionLabelProvider>();
        services.AddScoped<IPartyLookupLabelLocalizer, PartyLookupLabelLocalizer>();

        services.AddScoped<PartyAuditEntriesReader>();
        services.AddScoped<ILookupAddressTypesReader, LookupAddressTypesReader>();
        services.AddScoped<ILookupClassificationTypesReader, LookupClassificationTypesReader>();
        services.AddScoped<ILookupContactTypesReader, LookupContactTypesReader>();
        services.AddScoped<ILookupGendersReader, LookupGendersReader>();

        services.AddScoped<ILookupAddressTypesQueryHandler, LookupAddressTypesQueryHandler>();
        services.AddScoped<ILookupClassificationTypesQueryHandler, LookupClassificationTypesQueryHandler>();
        services.AddScoped<ILookupContactTypesQueryHandler, LookupContactTypesQueryHandler>();
        services.AddScoped<ILookupGendersQueryHandler, LookupGendersQueryHandler>();
        services.AddScoped<IPartyListAuditEntriesQueryHandler>(sp => new PartyListAuditEntriesQueryHandler(
            sp.GetRequiredService<PartyAuditEntriesReader>(),
            sp.GetRequiredService<IRequestContext>()
        ));

        services.AddPersonAggregateServices();
        services.AddOrganizationAggregateServices();

        return services;
    }
}
