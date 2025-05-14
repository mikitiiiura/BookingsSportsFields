using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using BookingsSportsFields.DataAccess.Repositories;

namespace BookingsSportsFields.DataAccess.Abstruction
{
    public interface IBookingsRepository
    {
        Task<Guid> AddAsync(BookingsEntity bookings);
        Task AddWithOutIdentityUser(BookingsEntity bookings);
        Task Delete(Guid id);
        Task<List<BookingsEntity>> GetAll();
        Task<List<BookingsEntity>> GetAllByUserID(Guid userId);
        Task<List<BookingsRepository.TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date);
        Task<bool> IsFieldAvailable(Guid sportsFieldId, DateTime startTime, DateTime endTime);
        Task Update(BookingsEntity bookings);
        Task UpdateStatus(Guid id, BookingStatus status);
    }
}