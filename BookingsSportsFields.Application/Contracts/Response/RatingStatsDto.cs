namespace BookingsSportsFields.Application.Contracts.Response;

public class RatingStatsDto
{
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<ReviewResponse> Reviews { get; set; } = new();
}