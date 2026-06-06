using Atlas.Identity.Application.Commands.SendWelcomeEmail;
using Atlas.Identity.Contracts.Commands.SendWelcomeEmail;
using Atlas.Outbox.Application.Targets;
using Atlas.Outbox.Targets.Identity.UserCreatedFromInvitation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Targets.Identity.DI;

public static class IdentitySendWelcomeEmailDependencyInjection
{
    public static IServiceCollection AddIdentitySendWelcomeEmailDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<ISendWelcomeEmailCommandHandler, SendWelcomeEmailCommandHandler>();
        services.AddScoped<ITargetHandler, SendWelcomeEmailTargetHandler>();

        return services;
    }
}
