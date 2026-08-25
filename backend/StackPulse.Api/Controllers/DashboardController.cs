using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackPulse.Api.DTOs.Dashboard;
using StackPulse.Api.Services.Interfaces;

namespace StackPulse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardSummaryDto>> GetDashboard(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
        var activity = await _dashboardService.GetRecentActivityAsync(cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Dashboard loaded successfully",
            data = new
            {
                summary,
                activity
            }
        });
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(new { success = true, message = "Dashboard summary loaded", data = summary });
    }
}
