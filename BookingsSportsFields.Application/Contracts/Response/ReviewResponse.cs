namespace BookingsSportsFields.Application.Contracts.Response;

public class ReviewResponse
{
    public Guid Id { get; set; }
    public byte Rating { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; } = "";
}