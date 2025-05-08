using System;
using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace BookingsSportsFields.DataAccess.Repositories
{
    public class BookingsRepository : IBookingsRepository
    {
        private readonly BookingsSportsFieldsDBContext _dBContext;
        private readonly ILogger<BookingsRepository> _logger;

        public BookingsRepository(BookingsSportsFieldsDBContext dBContext, ILogger<BookingsRepository> logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }

        public async Task<bool> IsFieldAvailable(Guid sportsFieldId, DateTime startTime, DateTime endTime)
        {
            _logger.LogInformation("Checking availability for SportsField ID: {SportsFieldId} from {StartTime} to {EndTime}",
                sportsFieldId, startTime, endTime);

            var conflictingBookings = await _dBContext.Bookings
                .Where(b => b.SportsFieldId == sportsFieldId &&
                           b.Status != BookingStatus.Cancelled && // Ігноруємо скасовані бронювання
                           ((startTime >= b.StartTime && startTime < b.EndTime) || // Новий початок всередині існуючого
                            (endTime > b.StartTime && endTime <= b.EndTime) || // Новий кінець всередині існуючого
                            (startTime <= b.StartTime && endTime >= b.EndTime))) // Новий період повністю містить існуючий
                .AsNoTracking()
                .AnyAsync();

            return !conflictingBookings;
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

        //public async Task Add(BookingsEntity bookings)
        //{
        //    _logger.LogInformation("Adding new bookings: {BookingsId}", bookings.Id);
        //    await _dBContext.Bookings.AddAsync(bookings);
        //    await _dBContext.SaveChangesAsync();
        //}

        public async Task Add(BookingsEntity bookings)
        {
            _logger.LogInformation("Adding new booking: {BookingsId}", bookings.Id);

            // Перевірка доступності
            bool isAvailable = await IsFieldAvailable(bookings.SportsFieldId, bookings.StartTime, bookings.EndTime);

            if (!isAvailable)
            {
                _logger.LogWarning("Field is not available for booking ID: {BookingsId}", bookings.Id);
                throw new Exception("The field is not available at the requested time");
            }

            await _dBContext.Bookings.AddAsync(bookings);
            await _dBContext.SaveChangesAsync();
        }

        public async Task<List<TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date)
        {
            _logger.LogInformation("Fetching available time slots for SportsField ID: {SportsFieldId} on {Date}",
                sportsFieldId, date.Date);

            // Отримуємо всі бронювання для цього майданчика в вказаний день
            var bookings = await _dBContext.Bookings
                .Where(b => b.SportsFieldId == sportsFieldId &&
                           b.StartTime.Date == date.Date &&
                           b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.StartTime)
                .AsNoTracking()
                .ToListAsync();

            // Стандартні години роботи (можна зробити налаштовуваними)
            TimeSpan openingTime = new TimeSpan(8, 0, 0); // 8:00
            TimeSpan closingTime = new TimeSpan(22, 0, 0); // 22:00
            TimeSpan slotDuration = new TimeSpan(0, 30, 0); // 30 хвилин

            List<TimeSlot> availableSlots = new List<TimeSlot>();
            DateTime currentSlotStart = date.Date.Add(openingTime);

            while (currentSlotStart.Add(slotDuration) <= date.Date.Add(closingTime))
            {
                DateTime currentSlotEnd = currentSlotStart.Add(slotDuration);

                bool isSlotAvailable = true;

                // Перевіряємо накладення з існуючими бронюваннями
                foreach (var booking in bookings)
                {
                    if ((currentSlotStart >= booking.StartTime && currentSlotStart < booking.EndTime) ||
                        (currentSlotEnd > booking.StartTime && currentSlotEnd <= booking.EndTime) ||
                        (currentSlotStart <= booking.StartTime && currentSlotEnd >= booking.EndTime))
                    {
                        isSlotAvailable = false;
                        break;
                    }
                }

                if (isSlotAvailable)
                {
                    availableSlots.Add(new TimeSlot { StartTime = currentSlotStart, EndTime = currentSlotEnd });
                }

                currentSlotStart = currentSlotEnd;
            }

            return availableSlots;
        }

        public class TimeSlot
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
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

            // Якщо змінився час, перевіряємо доступність
            if (existingBooking.StartTime != bookings.StartTime || existingBooking.EndTime != bookings.EndTime)
            {
                bool isAvailable = await IsFieldAvailable(bookings.SportsFieldId, bookings.StartTime, bookings.EndTime);

                // Якщо перевіряємо доступність для оновлення, потрібно виключити поточне бронювання
                // (воно може "конфліктувати" сам із собою)
                if (!isAvailable)
                {
                    _logger.LogWarning("Field is not available for updated time slot for booking ID: {BookingId}", bookings.Id);
                    throw new Exception("The field is not available at the requested time");
                }
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
        //public async Task Update(BookingsEntity bookings)
        //{
        //    _logger.LogInformation("Updating booking with ID: {BookingId}", bookings.Id);
        //    var existingBooking = await _dBContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookings.Id);

        //    if (existingBooking == null)
        //    {
        //        _logger.LogWarning("Booking with ID {BookingId} not found", bookings.Id);
        //        throw new Exception("Booking not found");
        //    }

        //    existingBooking.StartTime = bookings.StartTime;
        //    existingBooking.EndTime = bookings.EndTime;
        //    existingBooking.Status = bookings.Status;
        //    existingBooking.TotalPrice = bookings.TotalPrice;
        //    existingBooking.CreatedAt = bookings.CreatedAt;
        //    existingBooking.UserId = bookings.UserId;
        //    existingBooking.SportsFieldId = bookings.SportsFieldId;

        //    _dBContext.Bookings.Update(existingBooking);
        //    await _dBContext.SaveChangesAsync();
        //}

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
