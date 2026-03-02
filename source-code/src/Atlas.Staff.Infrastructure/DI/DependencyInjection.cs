using Atlas.SharedKernel.Application;
using Atlas.Staff.Application.StaffMemberApp.Persistence;
using Atlas.Staff.Application.StaffMembers.Queries.List;
using Atlas.Staff.Infrastructure.Persistence;
using Atlas.Staff.Infrastructure.Persistence.StaffMembers;
using Atlas.Staff.Infrastructure.Persistence.StaffMembers.Queries.List;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Staff.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddStaffModule(this IServiceCollection services)
    {
        services.AddScoped<IStaffMemberRepository, StaffMemberRepository>();
        services.AddScoped<IListStaffMembersReader, ListStaffMembersReader>();
        services.AddScoped<IUnitOfWork, StaffUnitOfWork>();

        return services;
    }
}