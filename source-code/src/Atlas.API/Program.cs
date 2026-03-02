using Atlas.API.Configs;
using Atlas.API.Security.Cors;
using Atlas.API.Security.Headers;
using Atlas.API.Security.OIDC;
using Atlas.API.Security.RateLimit;
using Atlas.API.Security.Tenancy;
using Atlas.BuildingBlocks.CQRS.Behaviors;
using Atlas.Identity.Application.Common;
using Atlas.Identity.Infrastructure.DI;
using Atlas.Identity.Infrastructure.Persistence;
using Atlas.Identity.Infrastructure.Persistence.Seed;
using Atlas.SharedKernel.Application;
using Atlas.Staff.Infrastructure.DI;
using Atlas.Staff.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

using IdentityAssemblyMarker = Atlas.Identity.Application.AssemblyMarker;
using StaffAssemblyMarker = Atlas.Staff.Application.AssemblyMarker;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

#region =====================================================
// TENANCY
#endregion

services.AddScoped<TenantContext>();
services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
services.AddScoped<ITenantProvider>(sp => sp.GetRequiredService<TenantContext>());

#region =====================================================
// DATABASE
#endregion

services.AddDbContext<IdentityDbContext>(o =>
    o.UseNpgsql(configuration.GetConnectionString("Default")));

services.AddDbContext<StaffDbContext>(o =>
    o.UseNpgsql(configuration.GetConnectionString("Default")));

#region =====================================================
// MODULE REGISTRATION
#endregion

services.AddIdentityModule();   // Repos, UoW, etc
services.AddStaffModule();

#region =====================================================
// CQRS + MEDIATR
#endregion

services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IdentityAssemblyMarker).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(StaffAssemblyMarker).Assembly);
});

services.AddValidatorsFromAssembly(typeof(IdentityAssemblyMarker).Assembly);
services.AddValidatorsFromAssembly(typeof(StaffAssemblyMarker).Assembly);

services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

#region =====================================================
// API CORE
#endregion

services.AddControllers();
services.AddAuthorization();
services.AddHealthChecks();
services.AddOpenApi();

services.Configure<FrontendConfig>(
    configuration.GetSection("Frontend"));

#region =====================================================
// SECURITY
#endregion

services.AddAppCors(configuration);
services.AddOidcMultiTenantAuthentication(configuration);
services.AddRateLimiting(configuration);

services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = false;
});

#region =====================================================
// SEEDING
#endregion

services.AddScoped<ISeeder, GlobalIdentitySeeder>();
services.AddScoped<SeederPipeline>();

#region =====================================================
// BUILD
#endregion

var app = builder.Build();

#region =====================================================
// MIGRATIONS (DEV ONLY)
#endregion

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var identityDb = scope.ServiceProvider
        .GetRequiredService<IdentityDbContext>();

    //await identityDb.Database.MigrateAsync();

    var pipeline = scope.ServiceProvider
        .GetRequiredService<SeederPipeline>();

    await pipeline.RunAsync(identityDb, scope.ServiceProvider);

    app.MapOpenApi();
}

#region =====================================================
// MIDDLEWARE
#endregion

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseCors("app");

app.UseAuthentication();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();