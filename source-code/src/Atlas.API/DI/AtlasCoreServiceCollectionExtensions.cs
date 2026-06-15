using Atlas.API.Errors;
using Atlas.BuildingBlocks.Application.Idempotency;
using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Localization;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Audit.Resources;
using Atlas.BuildingBlocks.Permissions;
using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.Audits.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants.Interfaces;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;
using Atlas.Identity.Application;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Application;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.API.DI;

internal static class AtlasCoreServiceCollectionExtensions
{
    internal static IServiceCollection AddAtlasCoreServices(this IServiceCollection services)
    {
        // Request context — scoped per HTTP request; dual-registered so both interfaces resolve to the same instance
        services.AddScoped<RequestContext>();
        services.AddScoped<IRequestContextSetter>(sp => sp.GetRequiredService<RequestContext>());
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());

        // Idempotency context — dual-registered
        services.AddScoped<MutableIdempotencyContext>();
        services.AddScoped<IIdempotencyContext>(sp => sp.GetRequiredService<MutableIdempotencyContext>());
        services.AddScoped<IIdempotencyContextSetter>(sp => sp.GetRequiredService<MutableIdempotencyContext>());

        services.AddHttpContextAccessor();
        services.AddProblemDetails();
        services.AddAtlasLocalization();
        services.AddScoped<IHttpResultMapper, HttpResultMapper>();

        // Localizers
        services.AddScoped<ErrorMessageLocalizer>();
        services.AddScoped<IErrorMessageLocalizer>(sp => sp.GetRequiredService<ErrorMessageLocalizer>());
        services.AddScoped<PermissionLabelLocalizer>();
        services.AddScoped<AuditLabelLocalizer>();
        services.AddScoped<IAuditLabelProvider, AuditActionLabelProvider>();

        // Outbox message building
        services.AddScoped<IOutboxMessageFactory, OutboxMessageFactory>();
        services.AddScoped<IOutboxMessageBuilder, OutboxMessageBuilder>();

        // Persistence pipeline — runs inside every SaveChanges via SavePipeline
        services.AddScoped<IAuditTrailService, AuditTrailService>();
        services.AddScoped<IEntityChangeStamper, EntityChangeStamper>();
        services.AddScoped<IEntityTenantStamper, EntityTenantStamper>();
        services.AddScoped<ISavePipeline, SavePipeline>();

        // FluentValidation — scanned per module assembly; ValidationDecorator picks them up automatically
        services.AddValidatorsFromAssemblyContaining<IdentityApplicationAssemblyMarker>();
        services.AddValidatorsFromAssemblyContaining<StaffApplicationAssemblyMarker>();

        return services;
    }
}
