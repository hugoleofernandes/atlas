using Atlas.BuildingBlocks.Email.DI;
using Atlas.BuildingBlocks.Observability;
using Atlas.Identity.Application.Emails;
using Atlas.Identity.Contracts.IntegrationEvents.Users;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Outbox.Infrastructure.Configuration;
using Atlas.Outbox.Infrastructure.DI;
using Atlas.Outbox.Infrastructure.Hangfire;
using Atlas.SharedKernel.Configuration;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
                .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
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
                    path: "logs/outbox-hangfire-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}"
                        + " {Properties:j}{NewLine}{Exception}"
                );
            }
        }
    );

    var configuration = builder.Configuration;
    var services = builder.Services;

    var hangfireOptions =
        configuration.GetSection("HangfireOutbox").Get<HangfireOutboxOptions>() ?? new HangfireOutboxOptions();
    var hangfireConnectionString = BuildHangfireConnectionString(configuration, hangfireOptions);
    ValidateDashboardAuth(hangfireOptions);

    if (hangfireOptions.Urls.Length > 0)
        builder.WebHost.UseUrls(hangfireOptions.Urls);

    services.AddAtlasObservability(configuration, builder.Environment);

    services.Configure<OutboxWorkerOptions>(configuration.GetSection("OutboxWorker"));
    services.Configure<HangfireOutboxOptions>(configuration.GetSection("HangfireOutbox"));
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

    services.AddScoped<ProcessIdentityOutboxHangfireJob>();
    services.AddScoped<ProcessStaffOutboxHangfireJob>();

    services.AddHangfire(cfg => cfg
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(storage => storage.UseNpgsqlConnection(hangfireConnectionString)));

    services.AddHangfireServer(serverOptions =>
    {
        serverOptions.ServerName = $"{hangfireOptions.ServerName}-identity";
        serverOptions.WorkerCount = 1;
        serverOptions.Queues = [HangfireOutboxQueues.Identity];
    });

    services.AddHangfireServer(serverOptions =>
    {
        serverOptions.ServerName = $"{hangfireOptions.ServerName}-staff";
        serverOptions.WorkerCount = 1;
        serverOptions.Queues = [HangfireOutboxQueues.Staff];
    });

    var app = builder.Build();

    await EnsureHangfireSchemaAsync(hangfireConnectionString, hangfireOptions.StorageSchema);

    app.UseSerilogRequestLogging(opts =>
    {
        opts.GetLevel = (ctx, _, _) =>
            ctx.Request.Path.StartsWithSegments(hangfireOptions.DashboardPath)
                ? Serilog.Events.LogEventLevel.Verbose
                : Serilog.Events.LogEventLevel.Information;
    });

    var dashboardOptions = new DashboardOptions
    {
        IsReadOnlyFunc = _ => hangfireOptions.DashboardAuth.ReadOnly,
    };

    if (hangfireOptions.DashboardAuth.Enabled)
    {
        dashboardOptions.Authorization =
        [
            new HangfireDashboardBasicAuthFilter(
                hangfireOptions.DashboardAuth.Username,
                hangfireOptions.DashboardAuth.Password,
                hangfireOptions.DashboardAuth.AllowInsecureHttp)
        ];
    }

    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Path.StartsWithSegments(hangfireOptions.DashboardPath))
        {
            var en = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            System.Globalization.CultureInfo.CurrentCulture = en;
            System.Globalization.CultureInfo.CurrentUICulture = en;
        }

        await next();
    });

    app.UseHangfireDashboard(hangfireOptions.DashboardPath, dashboardOptions);

    using (var scope = app.Services.CreateScope())
    {
        var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
        var backgroundJobs = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

        recurringJobs.AddOrUpdate<ProcessIdentityOutboxHangfireJob>(
            recurringJobId: "process-identity-outbox",
            methodCall: job => job.ExecuteAsync(JobCancellationToken.Null),
            cronExpression: hangfireOptions.RecurringCron,
            options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
        );

        recurringJobs.AddOrUpdate<ProcessStaffOutboxHangfireJob>(
            recurringJobId: "process-staff-outbox",
            methodCall: job => job.ExecuteAsync(JobCancellationToken.Null),
            cronExpression: hangfireOptions.RecurringCron,
            options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc }
        );

        if (hangfireOptions.TriggerJobsOnStartup)
        {
            backgroundJobs.Enqueue<ProcessIdentityOutboxHangfireJob>(job => job.ExecuteAsync(JobCancellationToken.Null));
            backgroundJobs.Enqueue<ProcessStaffOutboxHangfireJob>(job => job.ExecuteAsync(JobCancellationToken.Null));
        }
    }

    app.MapGet("/", () => Results.Redirect(hangfireOptions.DashboardPath));
    app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Outbox Hangfire host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

return 0;

static string BuildHangfireConnectionString(IConfiguration configuration, HangfireOutboxOptions options)
{
    var baseConnectionString =
        configuration.GetConnectionString(options.StorageConnectionStringName)
        ?? throw new InvalidOperationException(
            $"Connection string '{options.StorageConnectionStringName}' was not found."
        );

    var builder = new NpgsqlConnectionStringBuilder(baseConnectionString)
    {
        SearchPath = options.StorageSchema
    };

    return builder.ConnectionString;
}

static async Task EnsureHangfireSchemaAsync(string connectionString, string schemaName)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString)
    {
        SearchPath = string.Empty
    };

    await using var connection = new NpgsqlConnection(builder.ConnectionString);
    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText = $"""CREATE SCHEMA IF NOT EXISTS "{schemaName}" """;
    await command.ExecuteNonQueryAsync();
}

static void ValidateDashboardAuth(HangfireOutboxOptions options)
{
    if (!options.DashboardAuth.Enabled)
        return;

    if (string.IsNullOrWhiteSpace(options.DashboardAuth.Username))
        throw new InvalidOperationException("HangfireOutbox:DashboardAuth:Username is required when dashboard auth is enabled.");

    if (string.IsNullOrWhiteSpace(options.DashboardAuth.Password))
        throw new InvalidOperationException("HangfireOutbox:DashboardAuth:Password is required when dashboard auth is enabled.");
}
