using Atlas.Staff.Application.StaffMembers.Commands.CreateFromInvitation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Targets.Staff.DI;

public static class StaffCommandHandlersDependencyInjection
{
    public static IServiceCollection AddStaffCommandHandlersDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<
            ICreateStaffMemberFromInvitationCommandHandler,
            CreateStaffMemberFromInvitationCommandHandler
        >();

        return services;
    }
}
