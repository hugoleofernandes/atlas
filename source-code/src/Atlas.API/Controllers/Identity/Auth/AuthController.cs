using Atlas.API.Configs;
using Atlas.API.Errors;
using Atlas.BuildingBlocks.AspNetCore.Controllers;
using Atlas.BuildingBlocks.AspNetCore.HttpErrors;
using Atlas.SharedKernel.Application.Errors;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Atlas.API.Controllers.Identity.Auth;

[ApiController]
[Route("auth")]
public class AuthController(
    IConfiguration config,
    IOptions<FrontendConfig> frontOptions,
    ILogger<AuthController> logger,
    IErrorMessageLocalizer errorLocalizer,
    IHttpResultMapper resultMapper
) : AtlasController(errorLocalizer, resultMapper)
{
    private readonly IConfiguration _config = config;
    private readonly FrontendConfig _frontConfig = frontOptions.Value;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new
        {
            Message = "API de autenticação funcionando!",
            Tenants = _config.GetSection("Tenants").GetChildren().Select(c => c.Key).ToArray(),
            FrontendBaseUrl = _frontConfig.BaseUrl
        });
    }


    [HttpGet("login")]
    public IActionResult Login([FromQuery] string tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant))
            return ErrorResult(AuthErrors.Tenant.NameRequired);

        var tenants = _config.GetSection("Tenants")
            .GetChildren()
            .Select(c => c.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!tenants.Contains(tenant))
            return ErrorResult(AuthErrors.Tenant.Invalid);

        var home = $"{_frontConfig.BaseUrl}/admin/home";

        var props = new AuthenticationProperties
        {
            RedirectUri = home
        };

        return Challenge(props, tenant);
    }

    //[AllowAnonymous]
    //[HttpGet("logged-out-callback")]
    //public IActionResult LoggedOutCallback()
    //{
    //    // Depois do federated logout, o usuário foi removido do IdP corretamente.
    //    // Agora podemos devolver a SPA.
    //    return Redirect($"{_frontConfig.BaseUrl}/");
    //}

    [HttpPost("logout-spa")]
    [Authorize]
    public async Task<IActionResult> LogoutSpa()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { redirect = $"{_frontConfig.BaseUrl}/" });
    }
}
