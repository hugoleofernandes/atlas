using Atlas.API.Configs;
using Atlas.API.Security.Cors;
using Atlas.API.Security.Headers;
using Atlas.API.Security.OIDC;
using Atlas.API.Security.RateLimit;
using Atlas.API.Security.Tenancy;
using Atlas.Identity.Application.Abstractions;
using Atlas.Identity.Application.Common;
using Atlas.Identity.Application.UseCases.AuthorizeTenantLogin;
using Atlas.Identity.Infrastructure.Persistence;
using Atlas.Identity.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// DATABASE & TENANCY
// ==========================================================

builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
builder.Services.AddScoped<ITenantProvider>(sp => sp.GetRequiredService<TenantContext>());

builder.Services.AddDbContext<AtlasDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
    )
);

// ==========================================================
// CORE SERVICES
// ==========================================================

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

// ==========================================================
// CONFIGURATION
// ==========================================================

builder.Services.Configure<FrontendConfig>(
    builder.Configuration.GetSection("Frontend")
);

// ==========================================================
// SECURITY
// ==========================================================

builder.Services.AddAppCors(builder.Configuration);
builder.Services.AddOidcMultiTenantAuthentication(builder.Configuration);
builder.Services.AddRateLimiting(builder.Configuration);

builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = false;
});

// ==========================================================
// SEEDING PIPELINE
// ==========================================================

builder.Services.AddScoped<ISeeder, GlobalIdentitySeeder>();
builder.Services.AddScoped<ISeeder, TestEntitySeeder>();
builder.Services.AddScoped<SeederPipeline>();


builder.Services.AddScoped<IAuthorizeTenantLoginUseCase, AuthorizeTenantLoginUseCase>();

builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IIdentityUserRepository, IdentityUserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// ==========================================================
// BUILD APP
// ==========================================================

var app = builder.Build();

// ==========================================================
// MIGRATIONS + SEED (DEV ONLY)
// ==========================================================

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
    await db.Database.MigrateAsync();

    var pipeline = scope.ServiceProvider.GetRequiredService<SeederPipeline>();
    await pipeline.RunAsync(db, scope.ServiceProvider);
}

// ==========================================================
// DEVELOPMENT ENDPOINTS
// ==========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ==========================================================
// PRODUCTION SECURITY
// ==========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// ==========================================================
// MIDDLEWARE PIPELINE
// ==========================================================

app.UseHttpsRedirection();

app.UseSecurityHeaders();
app.UseRateLimiter();
app.UseCors("app");

app.UseAuthentication();
app.UseMiddleware<TenantResolverMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

//app.UseHttpsRedirection();


//app.MapHealthChecks("/health");

//app.MapGet("/", () => "Atlas API is running.");


//if (app.Environment.IsDevelopment())
//{
//    using var scope = app.Services.CreateScope();
//    var db = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
//    db.Database.Migrate();
//}


//app.MapGet("/", () => "Atlas API is running.");
//app.MapHealthChecks("/health");

//app.MapPost("/dev/tenants", async (AtlasDbContext db, CreateTenantRequest req) =>
//{
//    var tenant = new Tenant { Id = Guid.NewGuid(), Name = req.Name.Trim() };
//    db.Tenants.Add(tenant);
//    await db.SaveChangesAsync();
//    return Results.Created($"/dev/tenants/{tenant.Id}", tenant);
//});

//app.MapGet("/dev/tenants", async (AtlasDbContext db) =>
//    await db.Tenants.OrderBy(x => x.Name).ToListAsync());

//app.MapPatch("/dev/tenants/{id:guid}", async (AtlasDbContext db, Guid id, UpdateTenantRequest req) =>
//{
//    var tenant = await db.Tenants.FindAsync(id);
//    if (tenant is null) return Results.NotFound();

//    tenant.Name = req.Name.Trim();
//    await db.SaveChangesAsync();
//    return Results.Ok(tenant);
//});

//app.MapDelete("/dev/tenants/{id:guid}", async (AtlasDbContext db, Guid id) =>
//{
//    var tenant = await db.Tenants.FindAsync(id);
//    if (tenant is null) return Results.NotFound();

//    db.Tenants.Remove(tenant);
//    await db.SaveChangesAsync();
//    return Results.NoContent();
//});

//app.MapPost("/dev/users", async (AtlasDbContext db, CreateUserRequest req) =>
//{
//    var user = new User
//    {
//        Id = Guid.NewGuid(),
//        Provider = req.Provider,
//        ExternalId = req.ExternalId.Trim(),
//        Email = req.Email.Trim(),
//        DisplayName = req.DisplayName.Trim()
//    };

//    db.Users.Add(user);
//    await db.SaveChangesAsync();
//    return Results.Created($"/dev/users/{user.Id}", user);
//});

//app.MapPost("/dev/tenant-users", async (AtlasDbContext db, LinkTenantUserRequest req) =>
//{
//    var exists = await db.TenantUsers.AnyAsync(x => x.TenantId == req.TenantId && x.UserId == req.UserId);
//    if (exists) return Results.Conflict("User already linked to this tenant.");

//    var link = new TenantUser { TenantId = req.TenantId, UserId = req.UserId, Role = req.Role };
//    db.TenantUsers.Add(link);
//    await db.SaveChangesAsync();
//    return Results.Created("/dev/tenant-users", link);
//});

//app.MapDelete("/dev/tenant-users", async (AtlasDbContext db, Guid tenantId, Guid userId) =>
//{
//    var link = await db.TenantUsers.FindAsync(tenantId, userId);
//    if (link is null) return Results.NotFound();

//    db.TenantUsers.Remove(link);
//    await db.SaveChangesAsync();
//    return Results.NoContent();
//});


//record CreateTenantRequest(string Name);
//record UpdateTenantRequest(string Name);

//record CreateUserRequest(AuthProvider Provider, string ExternalId, string Email, string DisplayName);

//record LinkTenantUserRequest(Guid TenantId, Guid UserId, TenantRole Role);