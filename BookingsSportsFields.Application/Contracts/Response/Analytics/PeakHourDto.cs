namespace BookingsSportsFields.Application.Contracts.Response.Analytics;

public class PeakHourDto
{
    public string Hour { get; set; }
    public int BookingCount { get; set; }
    public double PercentOfTotal { get; set; }
}