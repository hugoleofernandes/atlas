using Atlas.BuildingBlocks.Email.DI;
using Atlas.Outbox.Catalog.DI.TargetCatalog;
using Atlas.Outbox.Targets.Identity.DI;
using Atlas.Outbox.Targets.Staff.DI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Catalog.DI;

public static class OutboxCatalogDependencyInjection
{
    public static IServiceCollection AddOutboxCatalogDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var dispatchMode = configuration["OutboxWorker:DispatchMode"];
        var isHttpDispatch = string.Equals(dispatchMode, "Http", StringComparison.OrdinalIgnoreCase);

        services.AddResendEmailService(configuration);

        // IDENTITY Module
        services.AddIdentitySendInvitationEmailDependencies(configuration);
        services.AddIdentitySendWelcomeEmailDependencies(configuration);
        //

        // STAFF Module
        services.AddStaffCommandHandlersDependencies(configuration);
        services.AddStaffTargetHandlersDependencies(configuration);

        if (!isHttpDispatch)
        {
            services.AddIdentityTargetCatalogDependencies(configuration);
            services.AddStaffTargetCatalogDependencies(configuration);
        }

        return services;
    }
}
