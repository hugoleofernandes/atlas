using Atlas.BuildingBlocks.Observability;
using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Outbox.Catalog.DI;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.Outbox.Infrastructure.DI;
using Atlas.Outbox.Worker.Hosting;
using Atlas.SharedKernel.Configuration;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration().MinimumLevel.Warning().WriteTo.Console().CreateBootstrapLogger();
DotEnvLoader.Load();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog(
            (ctx, services, cfg) =>
            {
                var otel =
                    ctx.Configuration.GetSection(ObservabilitySettings.SectionName).Get<ObservabilitySettings>()
                    ?? new ObservabilitySettings();

                cfg.MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithThreadId()
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
                    )
                    // Logs → Grafana Cloud Loki via OTLP (no-op se IsEnabled=false)
                    .WriteToAtlasObservability(otel, ctx.HostingEnvironment);

                if (!otel.IsEnabled)
                {
                    cfg.WriteTo.File(
                        path: "logs/outbox-worker-.txt",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}"
                            + " {Properties:j}{NewLine}{Exception}"
                    );
                }
            }
        )
        .ConfigureServices(
            (ctx, services) =>
            {
                var configuration = ctx.Configuration;
                var outboxWorkerOptions =
                    configuration.GetSection("OutboxWorker").Get<OutboxWorkerOptions>() ?? new OutboxWorkerOptions();

                // ── Observabilidade (Traces + Metrics → Grafana Cloud) ─────────────
                // Sem AddAspNetCoreInstrumentation — o worker não tem pipeline HTTP.
                services.AddAtlasObservability(configuration, ctx.HostingEnvironment);

                // ── Email ─────────────────────────────────────────────────────────
                // ── DbContexts — um por módulo que usa o outbox ───────────────────
                services.AddDbContext<IdentityDbContext>(o =>
                    o.UseNpgsql(configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention()
                );

                services.AddDbContext<StaffDbContext>(o =>
                    o.UseNpgsql(configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention()
                );

                // ── Assemblies dos tipos de integration events ────────────────────
                var integrationEventAssemblies = new[] { typeof(UserCreatedFromInvitationIntegrationEvent).Assembly };

                services.AddOutboxInfrastructureDependencies(configuration, integrationEventAssemblies);
                //if (!string.Equals(outboxWorkerOptions.DispatchMode, "Http", StringComparison.OrdinalIgnoreCase))
                //    services.AddIdentityEventsDirectTargets(configuration);

                // ── Módulos — infraestrutura por módulo (repos, UoW, idempotência) ──
                services.AddIdentityOutboxModuleDependencies();
                services.AddStaffOutboxModuleDependencies();

                // ── Bindings evento → handler ─────────────────────────────────────
                // Abra OutboxIntegrationDependencyInjection para ver todos os mappings.

                // ── Entry point — loop de polling ─────────────────────────────────
                // services.AddHostedService<OutboxWorkerHostedService>();  // V1 — mantido como referência
                if (outboxWorkerOptions.LegacyHostedServiceEnabled)
                    services.AddHostedService<OutboxWorkerHostedServiceV2>(); // V2 — workflow explícito
            }
        )
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "OutboxWorker terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
