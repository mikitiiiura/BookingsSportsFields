using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.Application.Contracts.Response;

namespace BookingsSportsFields.Application.InterfaceServices;

public interface IReviewService
{
    Task<Guid> AddReviewAsync(CreateReviewRequest request, Guid userId);
    Task<List<ReviewResponse>> GetReviewsForFieldAsync(Guid sportsFieldId);
    Task<double> GetAverageRatingAsync(Guid sportsFieldId);
}