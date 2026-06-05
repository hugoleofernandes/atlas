using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Permissions;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.Staff.Infrastructure.Labels;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.Staff.Infrastructure.Labels;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;
using Atlas.Staff.Application.StaffMembers.Queries.Audit.ListEntries;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Application.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Audit;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Repositories;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Staff.Infrastructure.DI;

public static class StaffDependencyInjection
{
    public static IServiceCollection AddStaffModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IAuditLabelProvider, StaffAuditLabelProvider>();
        services.AddScoped<IPermissionLabelProvider, StaffPermissionLabelProvider>();
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<ICreateStaffMemberFromInvitationCommandHandler, CreateStaffMemberFromInvitationCommandHandler>();
        services.AddScoped<IListStaffMembersReader, ListStaffMembersReader>();
        services.AddScoped<IStaffUnitOfWork, StaffUnitOfWork>();
        services.AddScoped<IIdempotencyService, IdempotencyService<StaffDbContext>>();

        // Audit reader registered as concrete type to avoid DI conflict with
        // other modules that also register IListAuditEntriesReader.
        services.AddScoped<StaffAuditEntriesReader>();

        // Factory lambda wires the Staff-specific reader into the shared audit handler
        // without exposing IListAuditEntriesReader in the root DI container.
        services.AddScoped<IStaffListAuditEntriesQueryHandler>(sp =>
            new StaffListAuditEntriesQueryHandler(
                sp.GetRequiredService<StaffAuditEntriesReader>(),
                sp.GetRequiredService<IRequestContext>()));

        return services;
    }
}
