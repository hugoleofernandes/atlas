using Atlas.BuildingBlocks.Observability;
using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Outbox.Infrastructure.DI;
using Atlas.Outbox.Worker.Hosting;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog((ctx, services, cfg) =>
        {
            var otel = ctx.Configuration
                .GetSection(ObservabilitySettings.SectionName)
                .Get<ObservabilitySettings>() ?? new ObservabilitySettings();

            cfg
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                // Logs → Grafana Cloud Loki via OTLP (no-op se IsEnabled=false)
                .WriteToAtlasObservability(otel, ctx.HostingEnvironment);

            if (!otel.IsEnabled)
            {
                cfg.WriteTo.File(
                    path: "logs/outbox-worker-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}" +
                        " {Properties:j}{NewLine}{Exception}");
            }
        })
        .ConfigureServices((ctx, services) =>
        {
            var configuration = ctx.Configuration;

            // ── Observabilidade (Traces + Metrics → Grafana Cloud) ─────────────
            // Sem AddAspNetCoreInstrumentation — o worker não tem pipeline HTTP.
            services.AddAtlasObservability(configuration, ctx.HostingEnvironment);

            // ── DbContexts — um por módulo que usa o outbox ───────────────────
            services.AddDbContext<IdentityDbContext>(o =>
                o.UseNpgsql(configuration.GetConnectionString("Default")));

            services.AddDbContext<StaffDbContext>(o =>
                o.UseNpgsql(configuration.GetConnectionString("Default")));

            // ── Assemblies dos tipos de integration events ────────────────────
            var integrationEventAssemblies = new[]
            {
                typeof(UserCreatedFromInvitationIntegrationEvent).Assembly
            };

            services.AddOutboxWorker(configuration, integrationEventAssemblies);

            // ── Módulos — um por módulo conforme integrados ───────────────────
            services.AddIdentityOutboxModuleDependencies();
            services.AddStaffOutboxModuleDependencies();

            // ── Entry point — loop de polling ─────────────────────────────────
            services.AddHostedService<OutboxWorkerHostedService>();
        })
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
