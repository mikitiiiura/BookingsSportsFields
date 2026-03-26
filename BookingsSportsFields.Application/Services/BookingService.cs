using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.Application.InterfaceServices;
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
            return await _bookingsRepository.GetAllBookingsForSportFieldByDate(userId, sportField, date);
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
            var startTime = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Utc);
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
            // Створення тимчасового користувача (якщо потрібно)
            var tempUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                UserName = request.PhoneNumber, // Або генерувати тимчасовий email
                PhoneNumber = request.PhoneNumber,
                Role = UserRole.Guest,
                CreatedAt = DateTime.UtcNow
            };

            await _userManager.CreateAsync(tempUser);

            // Створення бронювання
            var endTime = request.StartTime.AddMinutes(request.DurationMinutes);

            var booking = new BookingsEntity
            {
                Id = Guid.NewGuid(),
                SportsFieldId = request.SportFieldId,
                Comment = request.Comment,
                SportType = request.SportType,
                StartTime = request.StartTime,
                EndTime = endTime,
                Status = BookingStatus.Pending,
                TotalPrice = request.TotalPrice,
                UserId = tempUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _bookingsRepository.AddAsync(booking);
            return booking.Id;
        }
        public async Task<List<TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date, int sportType, Guid? instanceId = null)
        {
            return await _bookingsRepository.GetAvailableTimeSlots(sportsFieldId, date, (SportFieldsType)sportType, instanceId);
        }

        public async Task<bool> CheckAvailability(
            Guid sportsFieldId,
            DateTime startTime,
            int durationMinutes,
            int sportType,
            Guid? instanceId = null)  // ← вже є, але перевір
        {
            DateTime endTime = startTime.AddMinutes(durationMinutes);
            return await _bookingsRepository.IsFieldAvailable(
                sportsFieldId,
                startTime,
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
            return await _bookingsRepository.GetReservedReservationsForFieldOwnerCRM(ownerId, status, date, titleOfSportFild);
        }
        

    }
}

