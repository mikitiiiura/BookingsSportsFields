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
        
        /// <summary>
        /// We will delete this method but it is for test
        /// </summary>
        /// <returns>List BookingsEntity</returns>
        public async Task<List<BookingsEntity>> GetAll()
        {
            _logger.LogInformation("Fetching all bookings");
            return await _dBContext.Bookings
                .Include(b => b.User)
                .Include(b => b.SportsField)
                .ThenInclude(sf => sf.Location)
                .Include(b => b.SportsField)
                .ThenInclude(sf => sf.Owner)
                .AsNoTracking()
                .ToListAsync();
        }
        
        public async Task<List<BookingsEntity>> GetAllBookingsForSportFieldByDate(Guid userId, Guid sportField, DateTime date)
        {
            _logger.LogInformation("Fetching all bookings for manager to sport field by date");
    
            // Отримуємо початок і кінець дня
            var startOfDay = date.Date; // 00:00:00
            var endOfDay = date.Date.AddDays(1).AddTicks(-1); // 23:59:59.999
    
            return await _dBContext.Bookings
                .Where(b => b.UserId == userId && 
                            b.SportsFieldId == sportField &&
                            b.StartTime <= endOfDay && 
                            b.EndTime >= startOfDay)
                .Include(b => b.User)
                .Include(b => b.SportsField)
                // .ThenInclude(sf => sf.Location)
                .Include(b => b.SportsField)
                .ThenInclude(sf => sf.Owner)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<BookingsEntity>> GetAllByUserID(Guid userId)
        {
            _logger.LogInformation("Fetching bookings with User ID: {UserId}", userId);

            return await _dBContext.Bookings
                .Where(b => b.UserId == userId && b.Status != BookingStatus.Cancelled)
                .Include(b => b.User)
                .Include(b => b.SportsField)
                .ThenInclude(sf => sf.Location)
                .Include(b => b.SportsField)
                // .ThenInclude(sf => sf.TypesWithDetails)
                // .ThenInclude(t => t.WeeklySchedules)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Guid> AddAsync(BookingsEntity bookings)
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
            return bookings.Id;
        }
        
        public async Task DeleteBookingsOlderThanAsync(DateTime thresholdDate)
        {
            _logger.LogInformation("Deleting bookings older than {ThresholdDate}", thresholdDate);

            var oldBookings = await _dBContext.Bookings
                .Where(b => b.EndTime < thresholdDate)
                .ToListAsync();

            if (oldBookings.Any())
            {
                _dBContext.Bookings.RemoveRange(oldBookings);
                await _dBContext.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} old bookings", oldBookings.Count);
            }
            else
            {
                _logger.LogInformation("No old bookings to delete");
            }
        }

        public async Task<Guid> CancellationBooking(Guid bookingId)
        {
            _logger.LogInformation("Set cancel status for bookings with id: {BookingId}", bookingId);

            var bookingChanged = await _dBContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (bookingChanged == null)
                throw new Exception("Бронювання не знайдено");

            bookingChanged.Status = BookingStatus.Cancelled;
            await _dBContext.SaveChangesAsync();
            return bookingChanged.Id;
        }

        public Task<List<BookingsEntity>> GetFilteredBookingsCRM(Guid ownerId, int? status, DateTime? date, string? titleOfSportFild)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// NewerUse
        /// </summary>
        /// <param name="bookings"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task AddWithOutIdentityUser(BookingsEntity bookings)
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
        /// <summary>
        /// Get Available Time Slots
        /// </summary>
        /// <param name="sportsFieldId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<List<TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date)
        {
            _logger.LogInformation("Fetching available time slots for SportsField ID: {SportsFieldId} on {Date}",
                sportsFieldId, date.Date);

            var bookings = await _dBContext.Bookings
                .Where(b => b.SportsFieldId == sportsFieldId &&
                           b.StartTime.Date == date.Date &&
                           b.Status != BookingStatus.Cancelled)
                .OrderBy(b => b.StartTime)
                .AsNoTracking()
                .ToListAsync();

            TimeSpan openingTime = new TimeSpan(8, 0, 0);  // 8:00
            TimeSpan closingTime = new TimeSpan(22, 0, 0); // 22:00
            TimeSpan slotDuration = new TimeSpan(0, 30, 0); // 30 хв

            List<TimeSlot> availableSlots = new List<TimeSlot>();
            DateTime currentSlotStart = date.Date.Add(openingTime);

            while (currentSlotStart.Add(slotDuration) <= date.Date.Add(closingTime))
            {
                DateTime currentSlotEnd = currentSlotStart.Add(slotDuration);

                bool isSlotAvailable = !bookings.Any(b =>
                    (currentSlotStart >= b.StartTime && currentSlotStart < b.EndTime) ||
                    (currentSlotEnd > b.StartTime && currentSlotEnd <= b.EndTime) ||
                    (currentSlotStart <= b.StartTime && currentSlotEnd >= b.EndTime)
                );

                if (isSlotAvailable)
                {
                    availableSlots.Add(new TimeSlot
                    {
                        StartTime = currentSlotStart,
                        EndTime = currentSlotEnd
                    });
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


        /// <summary>
        /// Get reserved reservations for the field owner/admin
        /// </summary>
        public async Task<List<BookingsEntity>> GetReservedReservationsForFieldOwnerCRM(Guid ownerId, int? status, DateTime? date, string? titleOfSportFild)
        {
            _logger.LogInformation("Fetching filtered booking for owner ID: {OwnerId}", ownerId);
            try
            {
                var query =  _dBContext.Bookings
                    .Include(b => b.SportsField)
                    .ThenInclude(s => s.Owner)
                    .Where(b => b.SportsField.OwnerId == ownerId)
                    //.Include(b => b.User)
                    .AsNoTracking()
                    .AsQueryable();
                
                if (!string.IsNullOrWhiteSpace(titleOfSportFild))
                {
                    query = query.Where(b=>EF.Functions.Like(b.SportsField.Name, $"%{titleOfSportFild}%"));
                }
                if (status.HasValue)
                {
                    // query = query.Where(b => (int)b.Status == status.Value);
                    query = query.Where(b => b.Status == (BookingStatus)status.Value);
                }

                if (date.HasValue)
                {
                    var dayStart = date.Value.Date;
                    var dayEnd = dayStart.AddDays(1);

                    query = query.Where(b => b.StartTime >= dayStart && b.StartTime < dayEnd);
                }

                
                var bookings = await query.ToListAsync();
                return bookings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching filtered tasks for user ID: {OwnerId}", ownerId);
                throw;
            }
            
                
        }
        
        

    }

}
