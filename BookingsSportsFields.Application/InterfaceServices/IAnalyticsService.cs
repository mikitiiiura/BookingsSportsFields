using BookingsSportsFields.Application.Contracts.Response.Analytics;

namespace BookingsSportsFields.Application.InterfaceServices;

public interface IAnalyticsService
{
    Task<List<OccupancyDto>> GetOccupancyAsync(Guid sportsFieldId, DateTime date);
    Task<CancellationStatsDto> GetCancellationStatsAsync(Guid sportsFieldId, DateTime from, DateTime to);
    Task<List<RevenueDto>> GetRevenueAsync(Guid sportsFieldId, DateTime from, DateTime to);
    // Task<double> GetAverageRatingAsync(Guid sportsFieldId);
    Task<List<PeakHourDto>> GetPeakHoursAsync(Guid sportsFieldId, DateTime from, DateTime to);
}