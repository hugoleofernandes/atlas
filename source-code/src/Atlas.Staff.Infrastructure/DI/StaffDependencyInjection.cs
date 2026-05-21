using Atlas.Staff.Application.Abstractions;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Application.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Entities.StaffMembers.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Staff.Infrastructure.DI;

public static class StaffDependencyInjection
{
    public static IServiceCollection AddStaffModuleDependencies(this IServiceCollection services)
    {
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IListStaffMembersReader, ListStaffMembersReader>();
        services.AddScoped<IStaffUnitOfWork, StaffUnitOfWork>();

        return services;
    }
}