using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.Core;
using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BookingsSportsFields.DataAccess.Repositories.BookingsRepository;

namespace BookingsSportsFields.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingsRepository _bookingsRepository;
        private readonly ILogger<BookingService> _logger;
        private readonly UserManager<UserEntity> _userManager;

        public BookingService(IBookingsRepository bookingsRepository, ILogger<BookingService> logger, UserManager<UserEntity> userManager)
        {
            _bookingsRepository = bookingsRepository;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<List<BookingsEntity>> GetAllBooking()
        {
            _logger.LogInformation("Feaching Booking");
            return await _bookingsRepository.GetAll();
        }

        public async Task<List<BookingsEntity>> GetAllBookingsForSportFieldByDate(Guid userId, Guid sportField, DateTime date)
        {
            _logger.LogInformation("Fetching all bookings for sport field by date");
            return await _bookingsRepository.GetAllBookingsForSportFieldByDate(
                userId, sportField, UtcDateTimeHelper.UtcStartOfCalendarDay(date));
        }
        
        public async Task<List<BookingsEntity>> GetAllBookingsForSportFieldByDateForOwner(Guid sportFieldId, DateTime date)
        {
            _logger.LogInformation("Service: Fetching all bookings for sport field by date for owner");
            return await _bookingsRepository.GetAllBookingsForSportFieldByDateForOwner(
                sportFieldId, UtcDateTimeHelper.UtcStartOfCalendarDay(date));
        }

        public async Task<List<BookingsEntity>> GetBookingByUser(Guid userId)
        {
            _logger.LogInformation("Feaching Booking by User id: {userId}", userId);
            return await _bookingsRepository.GetAllByUserID(userId);
        }

        public async Task DeleteBooking(Guid bookingId)
        {
            _logger.LogInformation("Delete Booking by Booking id: {bookingId}", bookingId);
            await _bookingsRepository.Delete(bookingId);
        }

        // public async Task CreateBookingWithOutIdentityUser(BookingsEntity bookingsEntity)
        // {
        //
        //     await _bookingsRepository.AddWithOutIdentityUser(bookingsEntity);
        // }

        /// <summary>
        /// Для зареєстрованих
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Guid> CreateBookingAsync(CreateBookingRequest request)
        {
            var startTime = UtcDateTimeHelper.ToUtc(request.StartTime);
            var endTime = startTime.AddMinutes(request.DurationMinutes);

            var booking = new BookingsEntity
            {
                Id = Guid.NewGuid(),
                SportsFieldId = request.SportFieldId,
                SportsFieldInstanceId = request.SportsFieldInstanceId,   // ★★★ вже є
                Comment = request.Comment,
                SportType = request.SportType,
                StartTime = startTime,
                EndTime = endTime,
                Status = BookingStatus.Pending,
                TotalPrice = request.TotalPrice,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

            // ★★★ Перевірка доступності З instanceId ★★★
            bool isAvailable = await _bookingsRepository.IsFieldAvailable(
                booking.SportsFieldId,
                booking.StartTime,
                booking.EndTime,
                booking.SportType,
                booking.SportsFieldInstanceId   // ← це ключове!
            );

            if (!isAvailable)
            {
                _logger.LogWarning("Field not available: ID={Id}, Type={Type}, Instance={Instance}",
                    booking.Id, booking.SportType, booking.SportsFieldInstanceId);
                throw new Exception("The field is not available at the requested time for this sport type and instance");
            }

            await _bookingsRepository.AddAsync(booking);
            return booking.Id;
        }

        /// <summary>
        /// Для не зареєстрованого користувача
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
       public async Task<Guid> CreateGuestBookingAsync(CreateGuestBookingRequest request)
{
    _logger.LogInformation("Створення гостевого бронювання для {FullName}, телефон {Phone}, Instance={InstanceId}", 
        request.FullName, request.PhoneNumber, request.SportsFieldInstanceId);

    // Створюємо тимчасового користувача
    var tempUser = new UserEntity
    {
        Id = Guid.NewGuid(),
        UserName = "guest_" + Guid.NewGuid().ToString("N").Substring(0, 12), // правильний формат
        Email = $"guest_{Guid.NewGuid().ToString("N").Substring(0, 8)}@temp.com",
        FullName = request.FullName,
        PhoneNumber = request.PhoneNumber,
        Role = UserRole.Guest,
        CreatedAt = DateTime.UtcNow,
        EmailConfirmed = true,
        PhoneNumberConfirmed = true
    };

    var createResult = await _userManager.CreateAsync(tempUser);

    if (!createResult.Succeeded)
    {
        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
        _logger.LogError("Не вдалося створити тимчасового користувача: {Errors}", errors);
        throw new Exception($"Не вдалося створити гостя: {errors}");
    }

    // Створюємо бронювання
    var startUtc = UtcDateTimeHelper.ToUtc(request.StartTime);
    var endTime = startUtc.AddMinutes(request.DurationMinutes);

    var booking = new BookingsEntity
    {
        Id = Guid.NewGuid(),
        SportsFieldId = request.SportFieldId,
        SportsFieldInstanceId = request.SportsFieldInstanceId,
        Comment = request.Comment,
        SportType = request.SportType,
        StartTime = startUtc,
        EndTime = endTime,
        Status = BookingStatus.Pending,
        TotalPrice = request.TotalPrice,
        UserId = tempUser.Id,
        CreatedAt = DateTime.UtcNow
    };

    await _bookingsRepository.AddAsync(booking);

    _logger.LogInformation("Гостеве бронювання успішно створено. BookingId={BookingId}, UserId={UserId}, InstanceId={InstanceId}", 
        booking.Id, tempUser.Id, booking.SportsFieldInstanceId);

    return booking.Id;
}
        public async Task<List<TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date, int sportType, Guid? instanceId = null)
        {
            return await _bookingsRepository.GetAvailableTimeSlots(
                sportsFieldId, UtcDateTimeHelper.UtcStartOfCalendarDay(date), (SportFieldsType)sportType, instanceId);
        }

        public async Task<bool> CheckAvailability(
            Guid sportsFieldId,
            DateTime startTime,
            int durationMinutes,
            int sportType,
            Guid? instanceId = null)  // ← вже є, але перевір
        {
            var startUtc = UtcDateTimeHelper.ToUtc(startTime);
            DateTime endTime = startUtc.AddMinutes(durationMinutes);
            return await _bookingsRepository.IsFieldAvailable(
                sportsFieldId,
                startUtc,
                endTime,
                (SportFieldsType)sportType,
                instanceId  // ← передаємо instanceId
            );
        }
        public async Task DeleteOldBookingsAsync(DateTime thresholdDate)
        {
            _logger.LogInformation("Deleting old bookings older than {ThresholdDate}", thresholdDate);
            await _bookingsRepository.DeleteBookingsOlderThanAsync(thresholdDate);
        }
        
        public async Task<Guid> CancellationBooking(Guid bookingId)
        {
            _logger.LogInformation("Set cancel status for bookings with id: {BookingId}", bookingId);
            return await _bookingsRepository.CancellationBooking(bookingId);
        }

        public async Task<List<BookingsEntity>> GetReservedReservationsForFieldOwnerCRM(Guid ownerId, int? status, DateTime? date,
            string? titleOfSportFild)
        {
            _logger.LogInformation("Getting all bookings for owner ID: {OwnerId}", ownerId);
            var day = date.HasValue ? UtcDateTimeHelper.UtcStartOfCalendarDay(date.Value) : (DateTime?)null;
            return await _bookingsRepository.GetReservedReservationsForFieldOwnerCRM(ownerId, status, day, titleOfSportFild);
        }
        

    }
}

