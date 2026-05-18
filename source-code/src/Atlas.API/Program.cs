using Atlas.API.Configs;
using Atlas.API.Errors;
using Atlas.API.Filters;
using Atlas.API.Observability;
using Atlas.API.OpenApi;
using Atlas.API.Security;
using Atlas.API.Security.Bootstrap;
using Atlas.API.Security.Cors;
using Atlas.API.Security.Headers;
using Atlas.API.Security.OIDC;
using Atlas.API.Security.RateLimit;
using Atlas.API.Security.Authorization;
using Atlas.API.Security.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Atlas.BuildingBlocks.Application.OutboxMessages;
using Atlas.BuildingBlocks.Persistence;
using Atlas.BuildingBlocks.Persistence.Audits;
using Atlas.Identity.Application;
using Atlas.Identity.Infrastructure.DI;
using Atlas.Identity.Infrastructure.Persistence.DbContexts;
using Atlas.Identity.Infrastructure.Persistence.Seed;
using Atlas.SharedKernel.Application;
using Atlas.SharedKernel.Application.Commands;
using Atlas.SharedKernel.Application.OutboxMessages;
using Atlas.Staff.Application;
using Atlas.Staff.Infrastructure.DI;
using Atlas.Staff.Infrastructure.Persistence.DbContexts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

//
// ==========================================
// 🔹 SERILOG CONFIG (ANTES DO BUILDER)
// ==========================================
//

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    //.Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    var services = builder.Services;
    var configuration = builder.Configuration;

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
    services.AddIdentityOutboxWorkerSupport();
    //

    // STAFF
    services.AddStaffModuleDependencies();
    services.AddStaffOutboxWorkerSupport();
    //

    services.AddScoped<IOutboxMessageFactory, OutboxMessageFactory>();
    services.AddScoped<IOutboxMessageBuilder, OutboxMessageBuilder>();

    services.AddValidatorsFromAssemblyContaining<IdentityApplicationAssemblyMarker>();
    services.AddValidatorsFromAssemblyContaining<StaffApplicationAssemblyMarker>();

    services.AddScoped<IAuditService, AuditService>();


    //
    // ==========================================
    // CQRS + MEDIATR
    // ==========================================
    //

    //services.AddMediatR(cfg =>
    //{
    //    cfg.RegisterServicesFromAssembly(typeof(IdentityAssemblyMarker).Assembly);
    //    cfg.RegisterServicesFromAssembly(typeof(StaffApplicationAssemblyMarker).Assembly);
    //});

    //services.AddValidatorsFromAssembly(typeof(IdentityAssemblyMarker).Assembly);
    //services.AddValidatorsFromAssembly(typeof(StaffApplicationAssemblyMarker).Assembly);

    //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

    //services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();    

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
    services.AddOidcMultiTenantAuthentication(configuration);
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