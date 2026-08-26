using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using StackPulse.Api.Data;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IntegrationsController : ControllerBase
{
    private readonly StackPulseDbContext _db;
    private readonly MongoStackPulseContext _mongo;

    public IntegrationsController(StackPulseDbContext db, MongoStackPulseContext mongo)
    {
        _db = db;
        _mongo = mongo;
    }

    [HttpGet("jira/latest")]
    public async Task<IActionResult> GetLatestJira()
    {
        if (_mongo.IsConfigured)
        {
            return Ok(new { data = await GetMongoIntegrationPayload("Jira") });
        }

        var items = await _db.JiraIssues.OrderByDescending(x => x.CollectedAt).Take(50).ToListAsync();
        return Ok(new { data = items });
    }

    [HttpGet("bitbucket/latest")]
    public async Task<IActionResult> GetLatestBitbucket()
    {
        if (_mongo.IsConfigured)
        {
            return Ok(new { data = await GetMongoIntegrationPayload("Bitbucket") });
        }

        var items = await _db.BitbucketPullRequests.OrderByDescending(x => x.CollectedAt).Take(50).ToListAsync();
        return Ok(new { data = items });
    }

    private async Task<IEnumerable<object>> GetMongoIntegrationPayload(string provider)
    {
        var document = await _mongo.IntegrationSync
            .Find(Builders<BsonDocument>.Filter.Eq("provider", provider))
            .SortByDescending(x => x["completedAt"])
            .FirstOrDefaultAsync();

        if (document is null || !document.TryGetValue("payload", out var payload) || !payload.IsBsonArray)
        {
            return Array.Empty<object>();
        }

        return payload.AsBsonArray
            .OfType<BsonDocument>()
            .Select(BsonTypeMapper.MapToDotNetValue);
    }
}
