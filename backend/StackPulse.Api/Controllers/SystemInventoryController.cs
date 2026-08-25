using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackPulse.Api.Data;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemInventoryController : ControllerBase
{
    private readonly StackPulseDbContext _db;

    public SystemInventoryController(StackPulseDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
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
        var item = await _db.MachineInventories
            .Include(x => x.WindowsServices)
            .Include(x => x.InstalledSoftwares)
            .Include(x => x.Drives)
            .OrderByDescending(x => x.CollectedAt)
            .FirstOrDefaultAsync();

        if (item == null) return NotFound();
        return Ok(new { data = item });
    }
}
