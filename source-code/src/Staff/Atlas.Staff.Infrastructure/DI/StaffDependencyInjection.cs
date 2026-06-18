using Atlas.BuildingBlocks.Audit.Labels;
using Atlas.BuildingBlocks.Permissions;
using Atlas.BuildingBlocks.Persistence.Entities.Idempotency;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Handlers;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;
using Atlas.Staff.Application.StaffMembers.Commands.Register;
using Atlas.Staff.Application.StaffMembers.Commands.Terminate;
using Atlas.Staff.Application.StaffMembers.Commands.Update;
using Atlas.Staff.Application.StaffMembers.Queries.Audit.ListEntries;
using Atlas.Staff.Application.StaffMembers.Queries.GetById;
using Atlas.Staff.Application.StaffMembers.Queries.GetByPartyId;
using Atlas.Staff.Application.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Audit;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.GetById;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.GetByPartyId;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Repositories;
using Atlas.Staff.Infrastructure.Labels;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Atlas.Staff.Infrastructure.Seeders;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Staff.Infrastructure.DI;

public static class StaffDependencyInjection
{
    public static IServiceCollection AddStaffModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<StaffModuleSeeder>();
        services.AddScoped<IAuditLabelProvider, StaffAuditLabelProvider>();
        services.AddScoped<IPermissionLabelProvider, StaffPermissionLabelProvider>();

        // Repository
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();

        // Command handlers
        services.AddScoped<ICreateStaffMemberFromInvitationCommandHandler, CreateStaffMemberFromInvitationCommandHandler>();
        services.AddScoped<IRegisterStaffMemberCommandHandler, RegisterStaffMemberCommandHandler>();
        services.AddScoped<IUpdateStaffMemberCommandHandler, UpdateStaffMemberCommandHandler>();
        services.AddScoped<ITerminateStaffMemberCommandHandler, TerminateStaffMemberCommandHandler>();

        // Query handlers
        services.AddScoped<IGetStaffMemberByIdQueryHandler, GetStaffMemberByIdQueryHandler>();
        services.AddScoped<IGetStaffMemberByPartyIdQueryHandler, GetStaffMemberByPartyIdQueryHandler>();
        services.AddScoped<IListStaffMembersQueryHandler, ListStaffMembersQueryHandler>();

        // Readers
        services.AddScoped<IGetStaffMemberByIdReader, GetStaffMemberByIdReader>();
        services.AddScoped<IGetStaffMemberByPartyIdReader, GetStaffMemberByPartyIdReader>();
        services.AddScoped<IListStaffMembersReader, ListStaffMembersReader>();

        // Infrastructure
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
