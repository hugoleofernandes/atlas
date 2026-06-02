using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.BuildingBlocks.AspNetCore.Security.InternalApi;

public static class InternalApiKeyAuthenticationExtensions
{
    public static AuthenticationBuilder AddInternalApiKey(
        this AuthenticationBuilder builder,
        IConfiguration configuration)
        => builder.AddScheme<InternalApiKeyAuthenticationOptions, InternalApiKeyAuthenticationHandler>(
            InternalApiKeyDefaults.AuthenticationScheme,
            options =>
            {
                options.HeaderName = InternalApiHeaders.ApiKey;
                options.ApiKey = configuration["OutboxWorker:InternalApiKey"] ?? string.Empty;
                options.ServiceName = "outbox-worker";
            });

    public static AuthorizationOptions AddInternalApiPolicy(this AuthorizationOptions options)
    {
        options.AddPolicy(InternalApiKeyDefaults.PolicyName, policy =>
        {
            policy.AuthenticationSchemes.Add(InternalApiKeyDefaults.AuthenticationScheme);
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(InternalApiKeyDefaults.ActorTypeClaim, InternalApiKeyDefaults.ServiceActorType);
        });

        return options;
    }
}
