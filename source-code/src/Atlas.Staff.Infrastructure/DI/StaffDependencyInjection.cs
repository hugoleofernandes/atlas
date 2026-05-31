using Atlas.SharedKernel.Application;
using Atlas.Staff.Application.Abstractions;
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
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IListStaffMembersReader, ListStaffMembersReader>();
        services.AddScoped<IStaffUnitOfWork, StaffUnitOfWork>();

        // Audit reader registered as concrete type to avoid DI conflict with
        // other modules that also register IListAuditEntriesReader.
        services.AddScoped<StaffAuditEntriesReader>();

        // Factory lambda wires the Staff-specific reader into the handler without
        // exposing IListAuditEntriesReader in the shared DI container.
        //services.AddScoped<IStaffListAuditEntriesQueryHandler>(sp =>
        //    new StaffListAuditEntriesQueryHandler(
        //        sp.GetRequiredService<StaffAuditEntriesReader>(),
        //        sp.GetRequiredService<IRequestContext>()));

        //todo: rever codigo acima.

        return services;
    }
}
