using StackPulse.Api.DTOs.Dashboard;

namespace StackPulse.Api.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ActivityItemDto>> GetRecentActivityAsync(CancellationToken cancellationToken = default);
}
