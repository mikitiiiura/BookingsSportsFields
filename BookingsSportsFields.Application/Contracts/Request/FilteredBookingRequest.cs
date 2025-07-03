using System.ComponentModel.DataAnnotations;

namespace BookingsSportsFields.Application.Contracts.Request;

public record FilteredBookingRequest
(
    [Required] Guid OwnerId,
    int? Status, 
    DateTime? date, 
    string? titleOfSportFild

);