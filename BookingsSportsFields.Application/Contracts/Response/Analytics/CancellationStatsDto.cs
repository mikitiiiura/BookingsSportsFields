namespace BookingsSportsFields.Application.Contracts.Response.Analytics;

public class CancellationStatsDto
{
    public int TotalBookings { get; set; }
    public int Cancelled { get; set; }
    public double CancellationPercent { get; set; }
}