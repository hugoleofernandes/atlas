using Atlas.API.Configs;
using Atlas.API.Errors;
using Atlas.API.Observability;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.BuildingBlocks.AspNetCore.Observability;
using Atlas.API.Security.Bootstrap;
using Atlas.API.Security.Cors;
using Atlas.API.Security.Headers;
using Atlas.API.Security.OIDC;
using Atlas.API.Security.RateLimit;
using Atlas.BuildingBlocks.AspNetCore.Oidc;
using Atlas.BuildingBlocks.AspNetCore.Oidc.Providers.EntraId;
using Atlas.BuildingBlocks.AspNetCore.Security;
using Atlas.BuildingBlocks.AspNetCore.Security.Authorization;
using Atlas.BuildingBlocks.AspNetCore.Security.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.BuildingBlocks.Persistence;
using Atlas.BuildingBlocks.Persistence.Audits;
using Atlas.BuildingBlocks.Persistence.Tenancy;
using Atlas.Identity.Application;
using Atlas.Identity.Infrastructure.DI;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Persistence.Seed;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Application;
using Atlas.Staff.Infrastructure.DI;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;
using Atlas.BuildingBlocks.Persistence.OutboxMessages;

//
// ==========================================
// 🔹 SERILOG BOOTSTRAP (captura erros de startup)
// ==========================================
//

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    //
    // ==========================================
    // 🔹 SERILOG FULL CONFIG (hosted — acessa IConfiguration)
    // ==========================================
    //

    builder.Host.UseSerilog((context, services, config) =>
    {
        var otel = context.Configuration
            .GetSection(ObservabilitySettings.SectionName)
            .Get<ObservabilitySettings>() ?? new ObservabilitySettings();

        config
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            // Console sempre ativo — saída limpa para desenvolvimento
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        if (otel.IsEnabled)
        {
            // Logs → Grafana Cloud Loki via OTLP
            config.WriteTo.OpenTelemetry(o =>
            {
                o.Endpoint = $"{otel.Endpoint!.TrimEnd('/')}/v1/logs";
                o.Protocol = OtlpProtocol.HttpProtobuf;
                o.Headers = new Dictionary<string, string>
                {
                    ["Authorization"] = otel.ApiKey!
                };
                o.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"]    = otel.ServiceName,
                    ["service.version"] = otel.ServiceVersion,
                    ["deployment.environment"] = context.HostingEnvironment.EnvironmentName.ToLowerInvariant()
                };
            });
        }
        else
        {
            // Sem Grafana Cloud configurado: fallback para arquivo local
            config.WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}" +
                    " {Properties:j}{NewLine}{Exception}");
        }
    });

    var services = builder.Services;
    var configuration = builder.Configuration;

    //
    // ==========================================
    // 🔹 OBSERVABILITY (OTel traces + metrics → Grafana Cloud)
    // ==========================================
    //

    services.AddAtlasObservability(configuration, builder.Environment);

    //
    // ==========================================
    // CORE SERVICES
    // ==========================================
    //

    services.AddScoped<RequestContext>();
    services.AddScoped<IRequestContextSetter>(sp => sp.GetRequiredService<RequestContext>());
    services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());

    services.AddHttpContextAccessor();
    services.AddProblemDetails();
    services.AddLocalization(opts => opts.ResourcesPath = "Resources");
    services.AddScoped<ErrorMessageLocalizer>();
    services.AddScoped<IErrorMessageLocalizer>(sp => sp.GetRequiredService<ErrorMessageLocalizer>());

    //
    // ==========================================
    // DATABASE
    // ==========================================
    //

    services.AddDbContext<IdentityDbContext>(o =>
        o.UseNpgsql(configuration.GetConnectionString("Default")));

    services.AddDbContext<StaffDbContext>(o =>
        o.UseNpgsql(configuration.GetConnectionString("Default")));

    //
    // ==========================================
    // MODULE REGISTRATION
    // ==========================================
    //

    // IDENTITY
    services.AddIdentityModuleDependencies();
    services.AddTenantDependencies(builder.Configuration);
    //

    // STAFF
    services.AddStaffModuleDependencies();
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

    services.AddControllers(options =>
    {
        options.Filters.Add<ResultToHttpFilter>();
    }).ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory =
            ValidationProblemDetailsFactory.Create;
    });

    services.AddAuthorization();
    services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
    services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
    services.AddHealthChecks();

    services.AddOpenApi(options =>
    {
        options.AddOperationTransformer<ProblemDetailsOperationTransformer>();
    });

    services.AddEndpointsApiExplorer();

    services.Configure<FrontendConfig>(
        configuration.GetSection("Frontend"));

    //
    // ==========================================
    // SECURITY
    // ==========================================
    //

    services.AddAppCors(configuration);
    services.AddMultiTenantOidc(
        configuration,
        new EntraIdTenantConfigurator(AuthConstants.TenantHintCookie),
        AuthConstants.AuthCookie);
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

    services.AddScoped<ISeeder, GlobalIdentitySeeder>();
    services.AddScoped<SeederPipeline>();

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

        var identityDb = scope.ServiceProvider
            .GetRequiredService<IdentityDbContext>();

        var pipeline = scope.ServiceProvider
            .GetRequiredService<SeederPipeline>();

        await pipeline.RunAsync(identityDb, scope.ServiceProvider);

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
        opts.SetDefaultCulture("en")
            .AddSupportedCultures(supported)
            .AddSupportedUICultures(supported);
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

    app.MapControllers();

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
