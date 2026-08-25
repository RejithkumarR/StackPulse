using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackPulse.Api.Data;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationsController : ControllerBase
{
    private readonly StackPulseDbContext _db;

    public IntegrationsController(StackPulseDbContext db)
    {
        _db = db;
    }

    [HttpGet("jira/latest")]
    public async Task<IActionResult> GetLatestJira()
    {
        var items = await _db.JiraIssues.OrderByDescending(x => x.CollectedAt).Take(50).ToListAsync();
        return Ok(new { data = items });
    }

    [HttpGet("bitbucket/latest")]
    public async Task<IActionResult> GetLatestBitbucket()
    {
        var items = await _db.BitbucketPullRequests.OrderByDescending(x => x.CollectedAt).Take(50).ToListAsync();
        return Ok(new { data = items });
    }
}
