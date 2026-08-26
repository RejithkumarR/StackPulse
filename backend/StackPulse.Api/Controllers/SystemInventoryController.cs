using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using StackPulse.Api.Data;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemInventoryController : ControllerBase
{
    private readonly StackPulseDbContext _db;
    private readonly MongoStackPulseContext _mongo;

    public SystemInventoryController(StackPulseDbContext db, MongoStackPulseContext mongo)
    {
        _db = db;
        _mongo = mongo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (_mongo.IsConfigured)
        {
            var documents = await _mongo.MachineInventory
                .Find(FilterDefinition<BsonDocument>.Empty)
                .SortByDescending(x => x["collectedAt"])
                .Limit(100)
                .ToListAsync();

            return Ok(new { data = documents.Select(ToPlainObject) });
        }

        var items = await _db.MachineInventories
            .Include(x => x.WindowsServices)
            .Include(x => x.InstalledSoftwares)
            .Include(x => x.Drives)
            .OrderByDescending(x => x.CollectedAt)
            .ToListAsync();

        return Ok(new { data = items });
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatest()
    {
        if (_mongo.IsConfigured)
        {
            var document = await _mongo.MachineInventory
                .Find(FilterDefinition<BsonDocument>.Empty)
                .SortByDescending(x => x["collectedAt"])
                .FirstOrDefaultAsync();

            if (document is null) return NotFound();
            return Ok(new { data = ToPlainObject(document) });
        }

        var item = await _db.MachineInventories
            .Include(x => x.WindowsServices)
            .Include(x => x.InstalledSoftwares)
            .Include(x => x.Drives)
            .OrderByDescending(x => x.CollectedAt)
            .FirstOrDefaultAsync();

        if (item == null) return NotFound();
        return Ok(new { data = item });
    }

    private static object ToPlainObject(BsonDocument document) =>
        BsonTypeMapper.MapToDotNetValue(document);
}
