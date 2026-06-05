using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Targets.Staff.DI;

public static class StaffTargetsDependencyInjection
{
    public static IServiceCollection AddStaffTargetHandlersDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        //services.AddScoped<IDirectTargetHandler, SendWelcomeEmailDirectTargetHandler>();
        //services.AddScoped<IDirectTargetHandler, SendInvitationEmailDirectTargetHandler>();

        return services;
    }
}
