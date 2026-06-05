using Atlas.Identity.Application.Commands.SendInvitationEmail;
using Atlas.Identity.Contracts.Commands.SendInvitationEmail;
using Atlas.Outbox.Contracts.Targets;
using Atlas.Outbox.Targets.Identity.UserInvited;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Outbox.Targets.Identity.DI;

public static class IdentitySendInvitationEmailDependencyInjection
{
    public static IServiceCollection AddIdentitySendInvitationEmailDependencies(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<ISendInvitationEmailCommandHandler, SendInvitationEmailCommandHandler>();
        services.AddScoped<ITargetHandler, SendInvitationEmailTargetHandler>();

        return services;
    }
}
