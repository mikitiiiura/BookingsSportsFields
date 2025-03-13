using System;
using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace BookingsSportsFields.DataAccess.Repositories
{
    public class BookingsRepository
    {
        private readonly BookingsSportsFieldsDBContext _dBContext;
        private readonly ILogger<BookingsRepository> _logger;

        public BookingsRepository(BookingsSportsFieldsDBContext dBContext, ILogger<BookingsRepository> logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }

        public async Task<List<BookingsEntity>> GetAll()
        {
            _logger.LogInformation("Fetching all bookings");
            return await _dBContext.Bookings
                .Include(b => b.User)
                .Include(b => b.SportsField)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<BookingsEntity>> GetAllByID(Guid userId)
        {
            _logger.LogInformation("Fetching bookings with User ID: {UserId}", userId);
            return await _dBContext.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.User)
                .Include(b => b.SportsField)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task Add(BookingsEntity bookings)
        {
            _logger.LogInformation("Adding new bookings: {BookingsId}", bookings.Id);
            await _dBContext.Bookings.AddAsync(bookings);
            await _dBContext.SaveChangesAsync();
        }

        public async Task Update(BookingsEntity bookings)
        {
            _logger.LogInformation("Updating booking with ID: {BookingId}", bookings.Id);
            var existingBooking = await _dBContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookings.Id);

            if (existingBooking == null)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found", bookings.Id);
                throw new Exception("Booking not found");
            }

            existingBooking.StartTime = bookings.StartTime;
            existingBooking.EndTime = bookings.EndTime;
            existingBooking.Status = bookings.Status;
            existingBooking.TotalPrice = bookings.TotalPrice;
            existingBooking.CreatedAt = bookings.CreatedAt;
            existingBooking.UserId = bookings.UserId;
            existingBooking.SportsFieldId = bookings.SportsFieldId;

            _dBContext.Bookings.Update(existingBooking);
            await _dBContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            _logger.LogInformation("Deleting booking with ID: {BookingId}", id);

            var booking = await _dBContext.Bookings.FindAsync(id);
            if (booking == null)
            {
                _logger.LogWarning("Bookin with ID {BookingId} not found", id);
                return;
            }

            _dBContext.Bookings.Remove(booking);
            await _dBContext.SaveChangesAsync();
        }

        public async Task UpdateStatus(Guid id, BookingStatus status)
        {
            _logger.LogInformation("Updating booking status for ID: {BookingId}", id);
            var existingBooking = await _dBContext.Bookings.FirstOrDefaultAsync(b => b.Id == id);
            if (existingBooking == null)
            {
                _logger.LogWarning("Booking with ID {BookingId} not found", id);
                throw new Exception("Booking not found");
            }
            existingBooking.Status = status;
            _dBContext.Entry(existingBooking).Property(x => x.Status).IsModified = true; //перевірити----------------------------
            await _dBContext.SaveChangesAsync();
        }

    }

}
