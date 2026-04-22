using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.DataAccess.ModelEntity;
using BookingsSportsFields.DataAccess.Repositories;

namespace BookingsSportsFields.Application.InterfaceServices
{
    public interface IBookingService
    {
        Task<bool> CheckAvailability(
            Guid sportsFieldId, 
            DateTime startTime, 
            int durationMinutes, 
            int sportType,
            Guid? instanceId = null);
        Task<Guid> CreateBookingAsync(CreateBookingRequest request);
        // Task CreateBookingWithOutIdentityUser(BookingsEntity bookingsEntity);
        Task<Guid> CreateGuestBookingAsync(CreateGuestBookingRequest request);
        Task<List<BookingsEntity>> GetAllBooking();
        
        Task<List<BookingsEntity>> GetAllBookingsForSportFieldByDate(Guid userId, Guid sportField, DateTime date);

        Task<List<BookingsRepository.TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date, int sportType,
            Guid? instanceId = null);
        Task<List<BookingsEntity>> GetBookingByUser(Guid userId);

        Task DeleteBooking(Guid bookingId);
        Task DeleteOldBookingsAsync(DateTime thresholdDate);
        Task<Guid> CancellationBooking(Guid bookingId);

        Task<List<BookingsEntity>> GetReservedReservationsForFieldOwnerCRM(Guid ownerId, int? status, DateTime? date,
            string? titleOfSportFild);

        Task<List<BookingsEntity>> GetAllBookingsForSportFieldByDateForOwner(Guid ownerId, Guid sportFieldId, DateTime date);

        /// <summary>Підтвердити бронювання (тільки Pending). Власник майданчика.</summary>
        Task ConfirmBookingByManagerAsync(Guid bookingId, Guid ownerUserId);

        /// <summary>Скасувати бронювання власником: не пізніше ніж за 1 год до початку.</summary>
        Task CancelBookingByManagerAsync(Guid bookingId, Guid ownerUserId);

        /// <summary>Підтвердити всі очікуючі бронювання на майданчику.</summary>
        Task<int> ConfirmAllPendingForFieldAsync(Guid sportsFieldId, Guid ownerUserId);

        /// <summary>Увімкнути/вимкнути автопідтвердження нових бронювань для майданчика.</summary>
        Task SetAutoConfirmForFieldAsync(Guid sportsFieldId, bool enabled, Guid ownerUserId);

    }
}