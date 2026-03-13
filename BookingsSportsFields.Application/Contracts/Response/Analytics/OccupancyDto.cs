namespace BookingsSportsFields.Application.Contracts.Response.Analytics;

public class OccupancyDto
{
    public string Hour { get; set; }           // "09:00", "10:00"...
    public int PossibleSlots { get; set; }     // скільки слотів можливо (з розкладу)
    public int BookedSlots { get; set; }       // скільки заброньовано
    public double OccupancyPercent { get; set; }
}