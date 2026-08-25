using Microsoft.AspNetCore.Mvc;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public ActionResult GetHealth()
    {
        return Ok(new { status = "Healthy" });
    }
}
