using Atlas.BuildingBlocks.Email.Providers.Resend;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace Atlas.BuildingBlocks.Email.DI;

public static class EmailDependencyInjection
{
    /// <summary>
    /// Registers <see cref="IEmailService"/> backed by the Resend provider.
    ///
    /// Configuration section: <c>Email:Resend</c> (see <see cref="ResendEmailOptions"/>).
    ///
    /// Future: swap this method for <c>AddFallbackEmailService(primary, secondary)</c>
    /// when a multi-provider fallback strategy is needed — zero changes to call sites.
    /// </summary>
    public static IServiceCollection AddResendEmailService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ResendEmailOptions>(
            configuration.GetSection(ResendEmailOptions.SectionName));

        // Resend SDK — registers IResend backed by an HttpClient.
        services.AddOptions();
        services.Configure<ResendClientOptions>(o =>
        {
            var apiKey = configuration[$"{ResendEmailOptions.SectionName}:ApiKey"]
                ?? throw new InvalidOperationException(
                    "Email:Resend:ApiKey is required. Add it to appsettings or user-secrets.");
            o.ApiToken = apiKey;
        });
        services.AddHttpClient<ResendClient>();
        services.AddTransient<IResend, ResendClient>();

        // Our abstraction — scoped so it participates in the same DI scope as handlers.
        services.AddScoped<IEmailService, ResendEmailService>();

        return services;
    }
}
