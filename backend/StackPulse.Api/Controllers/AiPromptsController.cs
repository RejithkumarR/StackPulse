using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackPulse.Api.Data;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api/ai/prompts")]
public class AiPromptsController : ControllerBase
{
    private readonly StackPulseDbContext _db;
    private readonly IConfiguration _configuration;

    public AiPromptsController(StackPulseDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key, [FromHeader(Name = "X-Service-Token")] string? serviceToken, CancellationToken cancellationToken)
    {
        var expectedToken = Environment.GetEnvironmentVariable("AI_SERVICE_TOKEN")
            ?? _configuration["AiService:Token"];
        if (!string.IsNullOrEmpty(expectedToken) && serviceToken != expectedToken)
        {
            return Unauthorized();
        }

        var prompt = await _db.AiPromptTemplates
            .Where(x => x.Key == key && x.IsActive)
            .OrderByDescending(x => x.Version)
            .Select(x => new { key = x.Key, name = x.Name, template = x.Template, version = x.Version })
            .FirstOrDefaultAsync(cancellationToken);

        return prompt is null ? NotFound() : Ok(new { data = prompt });
    }
}