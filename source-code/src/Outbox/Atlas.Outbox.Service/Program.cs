using Atlas.BuildingBlocks.Email.DI;
using Atlas.BuildingBlocks.Observability;
using Atlas.Identity.Application.Emails;
using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Outbox.Application.Workflows.OutboxProcessing;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.Outbox.Infrastructure.DI;
using Atlas.Outbox.Service.Hosting;
using Atlas.SharedKernel.Configuration;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration().MinimumLevel.Warning().WriteTo.Console().CreateBootstrapLogger();
DotEnvLoader.Load();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(
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
                .Enrich.WithProperty("Module", "")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{Module:l}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteToAtlasObservability(otel, ctx.HostingEnvironment);

            if (!otel.IsEnabled)
            {
                cfg.WriteTo.File(
                    path: "logs/outbox-service-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{Module}] {Message:lj}"
                        + " {Properties:j}{NewLine}{Exception}"
                );
            }
        }
    );

    var configuration = builder.Configuration;
    var services = builder.Services;

    services.AddAtlasObservability(configuration, builder.Environment);

    services.Configure<OutboxWorkerOptions>(configuration.GetSection("OutboxWorker"));
    services.Configure<IdentityEmailOptions>(configuration.GetSection("IdentityEmail"));
    services.AddResendEmailService(configuration);

    services.AddDbContext<IdentityDbContext>(o =>
        o.UseNpgsql(configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention()
    );

    services.AddDbContext<StaffDbContext>(o =>
        o.UseNpgsql(configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention()
    );

    var integrationEventAssemblies = new[] { typeof(UserCreatedFromInvitationIntegrationEvent).Assembly };

    services.AddOutboxInfrastructureDependencies(configuration, integrationEventAssemblies);
    services.AddIdentityOutboxModuleDependencies();
    services.AddStaffOutboxModuleDependencies();

    services.AddSingleton<IHostedService>(sp =>
        new ModuleOutboxBackgroundService<IIdentityOutboxProcessingWorkflow>(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<IOptions<OutboxWorkerOptions>>(),
            "identity",
            sp.GetRequiredService<ILogger<ModuleOutboxBackgroundService<IIdentityOutboxProcessingWorkflow>>>()
        )
    );

    services.AddSingleton<IHostedService>(sp =>
        new ModuleOutboxBackgroundService<IStaffOutboxProcessingWorkflow>(
            sp.GetRequiredService<IServiceScopeFactory>(),
            sp.GetRequiredService<IOptions<OutboxWorkerOptions>>(),
            "staff",
            sp.GetRequiredService<ILogger<ModuleOutboxBackgroundService<IStaffOutboxProcessingWorkflow>>>()
        )
    );

    var app = builder.Build();

    app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Outbox Service host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;
