using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using BookingsSportsFields.DataAccess.Repositories;

namespace BookingsSportsFields.DataAccess.Abstruction
{
    public interface IBookingsRepository
    {
        Task<Guid> AddAsync(BookingsEntity bookings);
        // Task AddWithOutIdentityUser(BookingsEntity bookings);
        Task Delete(Guid id);
        Task<List<BookingsEntity>> GetAll();
        
        Task<List<BookingsEntity>> GetAllBookingsForSportFieldByDate(Guid userId, Guid sportField, DateTime date);
        Task<List<BookingsEntity>> GetAllByUserID(Guid userId);

        Task<List<BookingsRepository.TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date,
            SportFieldsType sportType, Guid? instanceId = null);

        Task<bool> IsFieldAvailable(
            Guid sportsFieldId,
            DateTime startTime,
            DateTime endTime,
            SportFieldsType sportType,
            Guid? instanceId = null,          // ★★★
            Guid? excludeBookingId = null);
        Task Update(BookingsEntity bookings);
        Task UpdateStatus(Guid id, BookingStatus status);
        Task DeleteBookingsOlderThanAsync(DateTime thresholdDate);
        Task<Guid> CancellationBooking(Guid bookingId);
        Task<List<BookingsEntity>> GetReservedReservationsForFieldOwnerCRM(Guid ownerId, int? status, DateTime? date, string? titleOfSportFild);
        
        /// <summary>
        /// Всі бронювання для майданчика за конкретну дату
        /// </summary>
        Task<List<BookingsEntity>> GetBookingsForFieldByDateAsync(Guid sportsFieldId, DateTime date);

        /// <summary>
        /// Всі бронювання за період (для скасувань, прибутку, пік-годин)
        /// </summary>
        Task<List<BookingsEntity>> GetBookingsForFieldByPeriodAsync(
            Guid sportsFieldId,
            DateTime from,
            DateTime to);

        /// <summary>
        /// Кількість бронювань по годинах за період (для пік-годин)
        /// </summary>
        Task<Dictionary<int, int>> GetHourlyBookingCountsAsync(
            Guid sportsFieldId,
            DateTime from,
            DateTime to);

        Task<bool> UserHasCompletedBookingAsync(Guid userId, Guid sportsFieldId);

        Task<List<BookingsEntity>> GetAllBookingsForSportFieldByDateForOwner(
            Guid sportFieldId,
            DateTime date);
    }
    
}