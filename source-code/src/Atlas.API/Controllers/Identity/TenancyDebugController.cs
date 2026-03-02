using Atlas.SharedKernel.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atlas.API.Controllers.Identity;

[ApiController]
[Route("debug/tenant")]
[Authorize]
public class TenancyDebugController : ControllerBase
{
    private readonly ITenantProvider _tenantProvider;

    public TenancyDebugController(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            TenantId = _tenantProvider.TenantId,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}