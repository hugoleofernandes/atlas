using Atlas.API.DI;
using Atlas.API.Seeding;
using Atlas.API.Security.Bootstrap;
using Atlas.API.Security.Cors;
using Atlas.API.Security.Headers;
using Atlas.API.Security.OIDC;
using Atlas.API.Security.RateLimit;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.BuildingBlocks.AspNetCore.Oidc;
using Atlas.BuildingBlocks.AspNetCore.Oidc.Providers.EntraId;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.BuildingBlocks.AspNetCore.Security.InternalApi;
using Atlas.BuildingBlocks.AspNetCore.Security.Tenancy;
using Atlas.BuildingBlocks.AspNetCore.Security.Xsrf;
using Atlas.BuildingBlocks.Email.DI;
using Atlas.BuildingBlocks.Observability;
using Atlas.BuildingBlocks.Permissions;
using Atlas.Identity.Application.Emails;
using Atlas.Identity.BffApi;
using Atlas.Identity.BffApi.Configs;
using Atlas.Identity.Domain;
using Atlas.Identity.Infrastructure.DI;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.InternalApi;
using Atlas.Identity.OutboxPublisher.DI;
using Atlas.Outbox.Infrastructure.DI;
using Atlas.Platform.BffApi;
using Atlas.Platform.Domain;
using Atlas.Platform.Infrastructure.DI;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.Platform.InternalApi;
using Atlas.SharedKernel.Configuration;
using Atlas.Staff.BffApi;
using Atlas.Staff.Contracts.Permissions;
using Atlas.Staff.Infrastructure.DI;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using Atlas.Staff.InternalApi;
using FastEndpoints;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

//
// ==========================================
// SERILOG BOOTSTRAP (captura erros de startup)
// ==========================================
//

Log.Logger = new LoggerConfiguration().MinimumLevel.Warning().WriteTo.Console().CreateBootstrapLogger();
DotEnvLoader.Load();

try
{
    var builder = WebApplication.CreateBuilder(args);

    //
    // ==========================================
    // SERILOG FULL CONFIG (hosted  IConfiguration)
    // ==========================================
    //

    builder.Host.UseSerilog(
        (context, services, config) =>
        {
            var otel =
                context.Configuration.GetSection(ObservabilitySettings.SectionName).Get<ObservabilitySettings>()
                ?? new ObservabilitySettings();

            config
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                // Console sempre ativo Ã¢â‚¬â€ saÃƒÂ­da limpa para desenvolvimento
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                // Logs Ã¢â€ â€™ Grafana Cloud Loki via OTLP (no-op se IsEnabled=false)
                .WriteToAtlasObservability(otel, context.HostingEnvironment);

            if (!otel.IsEnabled)
            {
                // Sem Grafana Cloud configurado: fallback para arquivo local
                config.WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}"
                        + " {Properties:j}{NewLine}{Exception}"
                );
            }
        }
    );

    var services = builder.Services;
    var configuration = builder.Configuration;

    //
    // ==========================================
    // OBSERVABILITY (OTel traces + metrics  Grafana Cloud)
    // ==========================================
    //

    services.AddAtlasObservability(
        configuration,
        builder.Environment,
        configureTracing: tracing =>
            tracing.AddAspNetCoreInstrumentation(o =>
            {
                o.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
                o.RecordException = true;
            }),
        configureMetrics: metrics =>
            metrics.AddAspNetCoreInstrumentation().AddMeter("Microsoft.AspNetCore.Server.Kestrel")
    );

    //
    // ==========================================
    // CORE SERVICES
    // ==========================================
    //

    services.AddAtlasCoreServices();

    // Permission catalog is persisted in the Identity database.
    // IPermissionCatalogReader is registered in IdentityDependencyInjection.

    //
    // ==========================================
    // DATABASE
    // ==========================================
    //

    services.AddDbContext<IdentityDbContext>(o =>
        o.UseNpgsql(configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention()
    );

    services.AddDbContext<StaffDbContext>(o =>
        o.UseNpgsql(configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention()
    );

    services.AddDbContext<PlatformDbContext>(o =>
        o.UseNpgsql(configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention()
    );

    //
    // ==========================================
    // MODULE REGISTRATION
    // ==========================================
    //

    // IDENTITY
    services.AddIdentityModuleDependencies();
    services.AddIdentityOutboxPublisherMappings();
    //

    // STAFF
    services.AddStaffModuleDependencies();

    // PLATFORM
    services.AddPlatformModuleDependencies();
    //

    // OUTBOX — management surface (triage/replay endpoints) + on-demand processing workflows
    services.AddOutboxManagementDependencies();
    services.AddOutboxOnDemandProcessingDependencies(configuration);

    //
    // ==========================================
    // API CORE
    // ==========================================
    //

    services.AddFastEndpoints(o =>
    {
        o.Assemblies =
        [
            typeof(IdentityBffApiAssemblyMarker).Assembly,
            typeof(StaffBffApiAssemblyMarker).Assembly,
            typeof(PlatformBffApiAssemblyMarker).Assembly,
            typeof(IdentityInternalApiAssemblyMarker).Assembly,
            typeof(StaffInternalApiAssemblyMarker).Assembly,
            typeof(PlatformInternalApiAssemblyMarker).Assembly,
        ];
    });

    services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = ValidationProblemDetailsFactory.Create;
        });

    services.AddAuthorization(options => options.AddInternalApiPolicy());
    services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
    services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
    services
        .AddHealthChecks()
        .AddNpgSql(configuration.GetConnectionString("Default")!, name: "postgres", tags: ["ready"])
        .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

    services.AddOpenApi(options =>
    {
        options.AddOperationTransformer<ProblemDetailsOperationTransformer>();
    });

    services.AddEndpointsApiExplorer();

    services.Configure<FrontendConfig>(configuration.GetSection("Frontend"));
    services.Configure<IdentityEmailOptions>(configuration.GetSection("IdentityEmail"));

    //
    // ==========================================
    // SECURITY
    // ==========================================
    //

    services.AddAppCors(configuration);
    services.AddBffXsrf();
    services.AddResendEmailService(configuration);
    services.AddMultiTenantOidc(
        configuration,
        new EntraIdTenantConfigurator(AuthConstants.TenantHintCookie),
        AuthConstants.AuthCookie
    );
    services.AddAuthentication().AddInternalApiKey(configuration);
    services.AddRateLimiting(configuration);

    services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
        options.Preload = false;
    });

    //
    // ==========================================
    // SEEDING
    // ==========================================
    //

    services.AddScoped<AtlasBootstrapSeeder>();

    //
    // ==========================================
    // BUILD
    // ==========================================
    //

    var app = builder.Build();

    //
    // ==========================================
    // DEV MIGRATIONS
    // ==========================================
    //

    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();

        var bootstrapSeeder = scope.ServiceProvider.GetRequiredService<AtlasBootstrapSeeder>();
        await bootstrapSeeder.RunAsync();

        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    //
    // ==========================================
    // MIDDLEWARE PIPELINE
    // ==========================================
    //

    if (!app.Environment.IsDevelopment())
        app.UseHsts();

    app.UseHttpsRedirection();

    app.UseRequestLocalization(opts =>
    {
        var supported = new[] { "en", "pt" };
        opts.SetDefaultCulture("en").AddSupportedCultures(supported).AddSupportedUICultures(supported);
        opts.ApplyCurrentCultureToResponseHeaders = true;
    });

    // CorrelationId PRIMEIRO
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Serilog HTTP logging
    app.UseSerilogRequestLogging();

    // Exception handling global
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseSecurityHeaders();
    app.UseRateLimiter();
    app.UseCors("app");

    app.UseAuthentication();
    app.UseMiddleware<TenantResolverMiddleware>();
    app.UseMiddleware<UserBootstrapMiddleware>();
    app.UseAuthorization();
    app.UseBffXsrf();

    app.UseFastEndpoints();
    app.MapControllers();

    app.MapHealthChecks(
            "/health/live",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            }
        )
        .AllowAnonymous();

    app.MapHealthChecks(
            "/health/ready",
            new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            }
        )
        .AllowAnonymous();

    // Pre-load OIDC metadata for all tenants in background right after startup,
    // so the first login request doesn't pay the cold-start cost.
    app.UseOidcMetadataWarmup(configuration);

    app.Run();
}
catch (HostAbortedException)
{
    // EF Core design-time tooling intentionally aborts the host after building it
    // to resolve DbContext services. This is not an application startup failure.
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
