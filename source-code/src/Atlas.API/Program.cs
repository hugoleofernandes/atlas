using Atlas.API.Configs;
using Atlas.API.Errors;
using Atlas.API.Filters;
using Atlas.API.Observability;
using Atlas.API.Security;
using Atlas.API.Security.Cors;
using Atlas.API.Security.Headers;
using Atlas.API.Security.OIDC;
using Atlas.API.Security.RateLimit;
using Atlas.API.Security.Tenancy;
using Atlas.BuildingBlocks.CQRS.Behaviors;
using Atlas.Identity.Application.UseCases.AuthorizeTenantLogin;
using Atlas.Identity.Infrastructure.DI;
using Atlas.Identity.Infrastructure.Persistence;
using Atlas.Identity.Infrastructure.Persistence.Seed;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Infrastructure.DI;
using Atlas.Staff.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

using IdentityAssemblyMarker = Atlas.Identity.Application.AssemblyMarker;
using StaffAssemblyMarker = Atlas.Staff.Application.AssemblyMarker;

//
// ==========================================
// 🔹 SERILOG CONFIG (ANTES DO BUILDER)
// ==========================================
//

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
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
    services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());

    services.AddHttpContextAccessor();
    services.AddProblemDetails();

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

    services.AddIdentityModule();
    services.AddStaffModule();

    //
    // ==========================================
    // CQRS + MEDIATR
    // ==========================================
    //

    services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(IdentityAssemblyMarker).Assembly);
        cfg.RegisterServicesFromAssembly(typeof(StaffAssemblyMarker).Assembly);
    });

    services.AddValidatorsFromAssembly(typeof(IdentityAssemblyMarker).Assembly);
    services.AddValidatorsFromAssembly(typeof(StaffAssemblyMarker).Assembly);

    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

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
    services.AddHealthChecks();
    services.AddOpenApi();

    services.Configure<FrontendConfig>(
        configuration.GetSection("Frontend"));

    services.AddScoped<IAuthorizeTenantLoginUseCase, AuthorizeTenantLoginUseCase>();

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
    }

    //
    // ==========================================
    // MIDDLEWARE PIPELINE
    // ==========================================
    //

    if (!app.Environment.IsDevelopment())
        app.UseHsts();

    app.UseHttpsRedirection();

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
    app.UseAuthorization();

    app.MapControllers();

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