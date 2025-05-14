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

        public async Task CreateBookingWithOutIdentityUser(BookingsEntity bookingsEntity)
        {

            await _bookingsRepository.AddWithOutIdentityUser(bookingsEntity);
        }

        /// <summary>
        /// Для зареєстрованих
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Guid> CreateBookingAsync(CreateBookingRequest request)
        {
            var endTime = request.StartTime.AddMinutes(request.DurationMinutes);

            var booking = new BookingsEntity
            {
                Id = Guid.NewGuid(),
                SportsFieldId = request.SportFieldId,
                Comment = request.Comment,
                StartTime = request.StartTime,
                EndTime = endTime,
                Status = BookingStatus.Pending,
                TotalPrice = request.TotalPrice,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow
            };

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
        public async Task<List<TimeSlot>> GetAvailableTimeSlots(Guid sportsFieldId, DateTime date)
        {
            return await _bookingsRepository.GetAvailableTimeSlots(sportsFieldId, date);
        }

        public async Task<bool> CheckAvailability(Guid sportsFieldId, DateTime startTime, int durationMinutes)
        {
            DateTime endTime = startTime.AddMinutes(durationMinutes);
            return await _bookingsRepository.IsFieldAvailable(sportsFieldId, startTime, endTime);
        }

    }
}
