using System;
using System.Linq.Expressions;
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

        // 1. Оновлений IsFieldAvailable (тепер з типом спорту)
      public async Task<bool> IsFieldAvailable(
    Guid sportsFieldId,
    DateTime startTime,
    DateTime endTime,
    SportFieldsType sportType,
    Guid? instanceId = null,
    Guid? excludeBookingId = null)
{
    const int bufferMinutes = 15;

    var query = _dBContext.Bookings
        .Where(b => b.SportsFieldId == sportsFieldId &&
                    b.Status != BookingStatus.Cancelled &&
                    b.SportType == sportType);

    if (instanceId.HasValue)
    {
        query = query.Where(b => b.SportsFieldInstanceId == instanceId.Value);
        _logger.LogInformation("Фільтр IsFieldAvailable по instanceId: {InstanceId}", instanceId);
    }
    else
    {
        _logger.LogInformation("IsFieldAvailable без instanceId — весь тип");
    }

    if (excludeBookingId.HasValue)
    {
        query = query.Where(b => b.Id != excludeBookingId.Value);
    }

    var bookings = await query.ToListAsync();

    _logger.LogInformation(
        "Перевірка: {Start} → {End} (buffer +{Buffer} хв), бронювань: {Count}, Instance: {InstanceId}",
        startTime.ToString("HH:mm"), endTime.ToString("HH:mm"), bufferMinutes, bookings.Count, instanceId
    );

    bool hasConflict = false;

    foreach (var b in bookings)
    {
        var existingEffectiveEnd = b.EndTime.AddMinutes(bufferMinutes);

        bool conflict = startTime < existingEffectiveEnd && b.StartTime < endTime;

        if (conflict)
        {
            _logger.LogWarning(
                "КОНФЛІКТ! Нове: {NewStart} → {NewEnd} | Існуюче: {ExistStart} → {ExistEnd} (eff end: {EffEnd}), Instance: {InstId}",
                startTime.ToString("HH:mm"),
                endTime.ToString("HH:mm"),
                b.StartTime.ToString("HH:mm"),
                b.EndTime.ToString("HH:mm"),
                existingEffectiveEnd.ToString("HH:mm"),
                b.SportsFieldInstanceId
            );
            hasConflict = true;
        }
    }

    _logger.LogInformation("Результат: {Result}", !hasConflict ? "ДОСТУПНО" : "ЗАЙНЯТО");

    return !hasConflict;
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
                .Include(b => b.SportsFieldInstance)
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
                .ThenInclude(sf => sf.TypesWithDetails)
                // .ThenInclude(t => t.WeeklySchedules)
                .AsNoTracking()
                .ToListAsync();
        }

        // 3. Оновлений AddAsync (використовує новий IsFieldAvailable)
        public async Task<Guid> AddAsync(BookingsEntity bookings)
        {
            _logger.LogInformation("Adding new booking: ID={Id}, Type={Type}, Instance={InstanceId}",
                bookings.Id, bookings.SportType, bookings.SportsFieldInstanceId);

            bool isAvailable = await IsFieldAvailable(
                bookings.SportsFieldId,
                bookings.StartTime,
                bookings.EndTime,
                bookings.SportType,
                bookings.SportsFieldInstanceId   // ← передаємо!
            );

            if (!isAvailable)
            {
                throw new Exception("The field is not available at the requested time for this sport type and instance");
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
        // public async Task AddWithOutIdentityUser(BookingsEntity bookings)
        // {
        //     _logger.LogInformation("Adding new booking: {BookingsId}", bookings.Id);
        //
        //     // Перевірка доступності
        //     bool isAvailable = await IsFieldAvailable(bookings.SportsFieldId, bookings.StartTime, bookings.EndTime);
        //
        //     if (!isAvailable)
        //     {
        //         _logger.LogWarning("Field is not available for booking ID: {BookingsId}", bookings.Id);
        //         throw new Exception("The field is not available at the requested time");
        //     }
        //
        //     await _dBContext.Bookings.AddAsync(bookings);
        //     await _dBContext.SaveChangesAsync();
        // }
        /// <summary>
        /// Get Available Time Slots
        /// </summary>
        /// <param name="sportsFieldId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        // 2. Оновлений GetAvailableTimeSlots (тепер з типом)
       public async Task<List<TimeSlot>> GetAvailableTimeSlots(
    Guid sportsFieldId,
    DateTime date,
    SportFieldsType sportType,
    Guid? instanceId = null)
{
    _logger.LogInformation("Запит слотів: Field={FieldId}, Date={Date}, Type={Type}, Instance={InstanceId}",
        sportsFieldId, date.Date, sportType, instanceId);

    var query = _dBContext.Bookings
        .Where(b => b.SportsFieldId == sportsFieldId &&
                    b.StartTime.Date == date.Date &&
                    b.Status != BookingStatus.Cancelled &&
                    b.SportType == sportType);

    if (instanceId.HasValue)
    {
        query = query.Where(b => b.SportsFieldInstanceId == instanceId.Value);
        _logger.LogInformation("Фільтр по інстансу: {InstanceId}", instanceId);
    }
    else
    {
        _logger.LogInformation("Без фільтра інстансу — весь тип");
    }

    var bookings = await query
        .OrderBy(b => b.StartTime)
        .AsNoTracking()
        .ToListAsync();

    _logger.LogInformation("Знайдено бронювань після фільтра: {Count}", bookings.Count);

    const int slotDurationMinutes = 60;      // 1 година
    const int bufferMinutes = 30;            // 30 хв перерва між бронюваннями

    TimeSpan openingTime = new TimeSpan(8, 0, 0);
    TimeSpan closingTime = new TimeSpan(22, 0, 0);

    var availableSlots = new List<TimeSlot>();
    var current = date.Date + openingTime;

    while (current.AddMinutes(slotDurationMinutes) <= date.Date + closingTime)
    {
        var slotEnd = current.AddMinutes(slotDurationMinutes);

        bool isAvailable = !bookings.Any(b =>
        {
            var bookingEffectiveEnd = b.EndTime.AddMinutes(bufferMinutes);
            return
                (current < bookingEffectiveEnd && current >= b.StartTime) ||
                (b.StartTime < slotEnd && b.EndTime > current);
        });

        if (isAvailable)
        {
            availableSlots.Add(new TimeSlot
            {
                StartTime = current,
                EndTime = slotEnd
            });
        }

        // Переходимо на наступний можливий слот (з перервою)
        current = slotEnd.AddMinutes(bufferMinutes);
    }

    return availableSlots;
}
        

        public class TimeSlot
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public List<string> FreeInstanceNames { get; set; } = new(); // ★★★
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
                bool isAvailable = await IsFieldAvailable(
                    bookings.SportsFieldId,
                    bookings.StartTime,
                    bookings.EndTime,
                    existingBooking.SportType,          // тип беремо зі старого бронювання
                    excludeBookingId: existingBooking.Id   // ігноруємо себе
                );

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
        
        
        public async Task<List<BookingsEntity>> GetBookingsForFieldByDateAsync(Guid sportsFieldId, DateTime date)
        {
            var start = date.Date;
            var end = start.AddDays(1);

            return await _dBContext.Bookings
                .Where(b => b.SportsFieldId == sportsFieldId &&
                            b.StartTime >= start &&
                            b.StartTime < end)
                .ToListAsync();
        }

        public async Task<List<BookingsEntity>> GetBookingsForFieldByPeriodAsync(Guid sportsFieldId, DateTime from, DateTime to)
        {
            return await _dBContext.Bookings
                .Where(b => b.SportsFieldId == sportsFieldId &&
                            b.StartTime >= from &&
                            b.StartTime < to)
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetHourlyBookingCountsAsync(Guid sportsFieldId, DateTime from, DateTime to)
        {
            var bookings = await _dBContext.Bookings
                .Where(b => b.SportsFieldId == sportsFieldId &&
                            b.StartTime >= from &&
                            b.StartTime < to)
                .GroupBy(b => b.StartTime.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Hour, x => x.Count);

            return bookings;
        }
        
        public async Task<bool> UserHasCompletedBookingAsync(Guid userId, Guid sportsFieldId)
        {
            return await _dBContext.Bookings
                .AnyAsync(b => b.UserId == userId 
                               && b.SportsFieldId == sportsFieldId 
                               && b.Status == BookingStatus.Completed);
        }

    }

}
