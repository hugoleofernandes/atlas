using Atlas.API.Configs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Atlas.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(
    IConfiguration config,
    IOptions<FrontendConfig> frontOptions,
    ILogger<AuthController> logger
) : ControllerBase
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
            return BadRequest("Tenant é obrigatório.");

        var tenants = _config.GetSection("Tenants")
            .GetChildren()
            .Select(c => c.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!tenants.Contains(tenant))
            return BadRequest("Tenant inválido.");

        var home = $"{_frontConfig.BaseUrl}/admin/home";

        var props = new AuthenticationProperties
        {
            RedirectUri = home
        };

        return Challenge(props, tenant);
    }

    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var lab = HttpContext.Request.Cookies["mlab_lab"];

        if (string.IsNullOrWhiteSpace(lab))
            return Redirect(_frontConfig.BaseUrl);

        lab = lab.ToLowerInvariant();

        // 1) Remove cookie local (sessão do BFF)
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // 2) Logout federado no IdP (Azure Entra) pelo scheme do tenant/lab
        await HttpContext.SignOutAsync(
            lab,
            new AuthenticationProperties
            {
                RedirectUri = "/auth/logged-out-callback"
            });

        // O middleware do OIDC vai cuidar do redirect para /auth/logged-out-callback
        return new EmptyResult();
    }

    [AllowAnonymous]
    [HttpGet("logged-out-callback")]
    public IActionResult LoggedOutCallback()
    {
        // Depois do federated logout, o usuário foi removido do IdP corretamente.
        // Agora podemos devolver a SPA.
        return Redirect($"{_frontConfig.BaseUrl}/");
    }

    [HttpPost("logout-spa")]
    [Authorize]
    public async Task<IActionResult> LogoutSpa()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { redirect = "/" });
    }
}