using System.Diagnostics;
using Atlas.API.Errors;
using Atlas.API.Security.Bootstrap;
using Atlas.API.Security.Cors;
using Atlas.API.Security.Headers;
using Atlas.API.Security.OIDC;
using Atlas.API.Security.RateLimit;
using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.BuildingBlocks.Application.Seeding;
using Atlas.BuildingBlocks.Application.Idempotency;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.BuildingBlocks.AspNetCore.Oidc;
using Atlas.BuildingBlocks.AspNetCore.Oidc.Providers.EntraId;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.BuildingBlocks.AspNetCore.Security.Tenancy;
using Atlas.BuildingBlocks.AuditTrail.Labels;
using Atlas.BuildingBlocks.FastEndpoints;
using Atlas.BuildingBlocks.Observability;
using Atlas.BuildingBlocks.Persistence.Entities.Audits;
using Atlas.BuildingBlocks.Persistence.Entities.Audits.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges;
using Atlas.BuildingBlocks.Persistence.Entities.EntityChanges.Interfaces;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants;
using Atlas.BuildingBlocks.Persistence.Entities.Tenants.Interfaces;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves;
using Atlas.BuildingBlocks.Persistence.Pipelines.Saves.Interfaces;
using Atlas.Identity.API;
using Atlas.Identity.API.Configs;
using Atlas.Identity.Application;
using Atlas.Identity.Infrastructure.DI;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.OutboxPublisher.DI;
using Atlas.Platform.API;
using Atlas.Platform.Infrastructure.DI;
using Atlas.Platform.Infrastructure.Persistence.DbContexts;
using Atlas.SharedDomain.Resources.Audit;
using Atlas.SharedDomain.Resources.Permissions;
using Atlas.SharedDomain.Permissions;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Errors;
using Atlas.SharedKernel.Application.Idempotency;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.SharedKernel.Domain.Permissions;
using Atlas.Staff.API;
using Atlas.Staff.Application;
using Atlas.Staff.Infrastructure.DI;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using FastEndpoints;
using FluentValidation;
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
// 🔹 SERILOG BOOTSTRAP (captura erros de startup)
// ==========================================
//

Log.Logger = new LoggerConfiguration().MinimumLevel.Warning().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    //
    // ==========================================
    // 🔹 SERILOG FULL CONFIG (hosted — acessa IConfiguration)
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
                // Console sempre ativo — saída limpa para desenvolvimento
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                // Logs → Grafana Cloud Loki via OTLP (no-op se IsEnabled=false)
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
    // 🔹 OBSERVABILITY (OTel traces + metrics → Grafana Cloud)
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

    services.AddScoped<RequestContext>();
    services.AddScoped<IRequestContextSetter>(sp => sp.GetRequiredService<RequestContext>());
    services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());
    services.AddScoped<MutableIdempotencyContext>();
    services.AddScoped<IIdempotencyContext>(sp => sp.GetRequiredService<MutableIdempotencyContext>());
    services.AddScoped<IIdempotencyContextSetter>(sp => sp.GetRequiredService<MutableIdempotencyContext>());

    services.AddHttpContextAccessor();
    services.AddProblemDetails();
    services.AddLocalization(opts => opts.ResourcesPath = "Resources");
    services.AddScoped<ErrorMessageLocalizer>();
    services.AddScoped<IErrorMessageLocalizer>(sp => sp.GetRequiredService<ErrorMessageLocalizer>());
    services.AddScoped<IPermissionLabelProvider, SharedDomainPermissionLabelProvider>();
    services.AddScoped<PermissionLabelLocalizer>();
    services.AddScoped<IAuditLabelProvider, SharedDomainAuditLabelProvider>();
    services.AddScoped<AuditLabelLocalizer>();
    services.AddScoped<IHttpResultMapper, HttpResultMapper>();

    services.AddSingleton<IModulePermissions, IdentityModulePermissions>();
    services.AddSingleton<IModulePermissions, StaffPermissions>();
    services.AddSingleton<IModulePermissions, PlatformModulePermissions>();
    services.AddSingleton<IPermissionPolicy>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<PermissionPolicyService>>();
        var sw     = Stopwatch.StartNew();
        var modules = sp.GetServices<IModulePermissions>().ToList();
        var policy  = new PermissionPolicyService(modules);
        sw.Stop();

        logger.LogInformation(
            "Permission catalog built in {ElapsedMs} ms - {PermissionCount} codes, {GroupCount} groups, {ModuleCount} modules",
            sw.ElapsedMilliseconds,
            policy.All.Count,
            policy.Groups.Count,
            modules.Count);

        return policy;
    });

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

    services.AddScoped<IOutboxMessageFactory, OutboxMessageFactory>();
    services.AddScoped<IOutboxMessageBuilder, OutboxMessageBuilder>();

    services.AddValidatorsFromAssemblyContaining<IdentityApplicationAssemblyMarker>();
    services.AddValidatorsFromAssemblyContaining<StaffApplicationAssemblyMarker>();

    services.AddScoped<IAuditTrailService, AuditTrailService>();
    services.AddScoped<IEntityChangeStamper, EntityChangeStamper>();
    services.AddScoped<IEntityTenantStamper, EntityTenantStamper>();
    services.AddScoped<ISavePipeline, SavePipeline>();

    //
    // ==========================================
    // API CORE
    // ==========================================
    //

    services.AddFastEndpoints(o =>
    {
        o.Assemblies =
        [
            typeof(IdentityApiAssemblyMarker).Assembly,
            typeof(StaffApiAssemblyMarker).Assembly,
            typeof(PlatformApiAssemblyMarker).Assembly,
        ];
    });

    services
        .AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = ValidationProblemDetailsFactory.Create;
        });

    services.AddAuthorization();
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

    //
    // ==========================================
    // SECURITY
    // ==========================================
    //

    services.AddAppCors(configuration);
    services.AddMultiTenantOidc(
        configuration,
        new EntraIdTenantConfigurator(AuthConstants.TenantHintCookie),
        AuthConstants.AuthCookie
    );
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

    services.AddScoped<SeedOrchestrator>();

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

        var orchestrator = scope.ServiceProvider.GetRequiredService<SeedOrchestrator>();
        await orchestrator.RunAsync(scope.ServiceProvider);

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

    // 🔹 CorrelationId PRIMEIRO
    app.UseMiddleware<CorrelationIdMiddleware>();

    // 🔹 Serilog HTTP logging
    app.UseSerilogRequestLogging();

    // 🔹 Exception handling global
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseSecurityHeaders();
    app.UseRateLimiter();
    app.UseCors("app");

    app.UseAuthentication();
    app.UseMiddleware<TenantResolverMiddleware>();
    app.UseMiddleware<UserBootstrapMiddleware>();
    app.UseAuthorization();

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
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
