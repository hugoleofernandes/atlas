using Atlas.Contracts.Tenants.IntegrationEvents;
using Atlas.Identity.Infrastructure.DI;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.OutboxWorker.DI;
using Atlas.Staff.Infrastructure.DI;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog((ctx, services, cfg) => cfg
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console())
        .ConfigureServices((ctx, services) =>
        {
            var configuration = ctx.Configuration;

            // DbContexts — um por módulo que usa o outbox
            services.AddDbContext<IdentityDbContext>(o =>
                o.UseNpgsql(configuration.GetConnectionString("Default")));

            services.AddDbContext<StaffDbContext>(o =>
                o.UseNpgsql(configuration.GetConnectionString("Default")));

            // Assemblies onde os tipos de integration events estão definidos.
            // Adicionar a assembly de cada módulo que publica/consome via outbox.
            var integrationEventAssemblies = new[]
            {
                typeof(UserCreatedFromInvitationIntegrationEvent).Assembly
            };

            services.AddOutboxWorker(configuration, integrationEventAssemblies);

            // Módulos — um por módulo conforme forem sendo integrados
            services.AddIdentityOutboxWorkerSupport();
            services.AddStaffOutboxWorkerSupport();
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
