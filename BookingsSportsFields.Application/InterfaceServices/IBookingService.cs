using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.DataAccess.ModelEntity;
using BookingsSportsFields.DataAccess.Repositories;

namespace BookingsSportsFields.Application.InterfaceServices
{
    public interface IBookingService
    {
        Task<bool> CheckAvailability(Guid sportsFieldId, DateTime startTime, int durationMinutes);
        Task<Guid> CreateBookingAsync(CreateBookingRequest request);
        Task CreateBookingWithOutIdentityUser(BookingsEntity bookingsEntity);
        Task<Guid> CreateGuestBookingAsync(CreateGuestBookingRequest request);
        Task<List<BookingsEntity>> GetAllBooking();
        Task<List<BookingsRepository.TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date);
        Task<List<BookingsEntity>> GetBookingByUser(Guid userId);

        Task DeleteBooking(Guid bookingId);
    }
}