using BookingsSportsFields.DataAccess.ModelEntity;

namespace BookingsSportsFields.DataAccess.Abstruction;

public interface IReviewsRepository
{
    Task<List<ReviewsEntity>> GetAll();
    Task<bool> HasReviewForBookingAsync(Guid bookingId, Guid userId);
}

