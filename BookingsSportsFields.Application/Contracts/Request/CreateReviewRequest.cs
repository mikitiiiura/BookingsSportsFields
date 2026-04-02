namespace BookingsSportsFields.Application.Contracts.Request;

public class CreateReviewRequest
{
    public Guid SportsFieldId { get; set; }
    public Guid UserId { get; set; }
    public Guid? BookingId { get; set; }     // ← додаємо
    public byte Rating { get; set; }        // 1-5
    public string? Comment { get; set; }
}