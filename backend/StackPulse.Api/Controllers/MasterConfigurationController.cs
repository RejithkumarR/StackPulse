using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackPulse.Api.Data;
using StackPulse.Api.DTOs.MasterConfiguration;
using StackPulse.Api.Models;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api/master-configuration")]
public class MasterConfigurationController : ControllerBase
{
    private readonly StackPulseDbContext _db;

    public MasterConfigurationController(StackPulseDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<object>> Get(CancellationToken cancellationToken)
    {
        var computers = await _db.ComputerMasters
            .OrderBy(x => x.Hostname)
            .Select(x => new ComputerMasterDto
            {
                Id = x.Id,
                Hostname = x.Hostname,
                AssetTag = x.AssetTag,
                Owner = x.Owner,
                Environment = x.Environment,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var integrations = await _db.IntegrationAccesses
            .OrderBy(x => x.Provider)
            .ThenBy(x => x.DisplayName)
            .Select(x => new IntegrationAccessDto
            {
                Id = x.Id,
                Provider = x.Provider,
                DisplayName = x.DisplayName,
                BaseUrl = x.BaseUrl,
                ProjectKey = x.ProjectKey,
                Username = x.Username,
                SecretReference = x.SecretReference,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(new { data = new MasterConfigurationDto { Computers = computers, Integrations = integrations } });
    }

    [HttpPost("computers")]
    public async Task<ActionResult<object>> SaveComputer(ComputerMasterDto dto, CancellationToken cancellationToken)
    {
        var entity = dto.Id.HasValue
            ? await _db.ComputerMasters.FirstOrDefaultAsync(x => x.Id == dto.Id.Value, cancellationToken)
            : null;

        if (entity is null)
        {
            entity = new ComputerMaster { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            _db.ComputerMasters.Add(entity);
        }

        entity.Hostname = dto.Hostname.Trim();
        entity.AssetTag = dto.AssetTag;
        entity.Owner = dto.Owner;
        entity.Environment = dto.Environment;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        dto.Id = entity.Id;
        return Ok(new { data = dto });
    }

    [HttpPost("integrations")]
    public async Task<ActionResult<object>> SaveIntegration(IntegrationAccessDto dto, CancellationToken cancellationToken)
    {
        var entity = dto.Id.HasValue
            ? await _db.IntegrationAccesses.FirstOrDefaultAsync(x => x.Id == dto.Id.Value, cancellationToken)
            : null;

        if (entity is null)
        {
            entity = new IntegrationAccess { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            _db.IntegrationAccesses.Add(entity);
        }

        entity.Provider = dto.Provider.Trim();
        entity.DisplayName = dto.DisplayName.Trim();
        entity.BaseUrl = dto.BaseUrl.Trim();
        entity.ProjectKey = dto.ProjectKey;
        entity.Username = dto.Username;
        entity.SecretReference = dto.SecretReference;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        dto.Id = entity.Id;
        return Ok(new { data = dto });
    }
}
