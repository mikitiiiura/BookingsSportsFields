using System.Security.Claims;
using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.Application.Contracts.Response;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.AspNetCore.Authorization;
// using BookingsSportsFields.Core.Model;
using Microsoft.AspNetCore.Mvc;
using static BookingsSportsFields.DataAccess.Repositories.BookingsRepository;

namespace BookingsSportsFields.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<BookingResponse>>> GetAll()
        {
            var booking = await _bookingService.GetAllBooking();

            var response = booking.Select(x => new BookingResponse(x)).ToList();

            return Ok(response);
        }
        
        [HttpGet("GetAllBookingsForSportFieldByDate")]
        public async Task<ActionResult<List<BookingResponse>>> GetAllBookingsForSportFieldByDate(Guid userId, Guid sportField, DateTime date)
        {
            var booking = await _bookingService.GetAllBookingsForSportFieldByDate(userId, sportField, date);

            var response = booking.Select(x => new BookingResponse(x)).ToList();

            return Ok(response);
        }
        
        [HttpGet("GetAllBookingsForSportFieldByDateForOwner")]
        [Authorize]
        public async Task<ActionResult<List<BookingResponse>>> GetAllBookingsForSportFieldByDateForOwner(
            [FromQuery] Guid sportFieldId,
            [FromQuery] DateTime date)
        {
            _logger.LogInformation(
                "Owner bookings request auth={IsAuth}, nameIdentifier={NameId}, name={Name}, role={Role}, sportFieldId={FieldId}, date={Date}",
                User?.Identity?.IsAuthenticated ?? false,
                User?.FindFirstValue(ClaimTypes.NameIdentifier),
                User?.FindFirstValue(ClaimTypes.Name),
                User?.FindFirstValue(ClaimTypes.Role),
                sportFieldId,
                date);

            var ownerId = ParseUserId(User);
            if (ownerId == null)
                return Unauthorized(new { message = "Missing or invalid NameIdentifier claim." });

            var bookings = await _bookingService.GetAllBookingsForSportFieldByDateForOwner(ownerId.Value, sportFieldId, date);

            var response = bookings.Select(x => new BookingResponse(x)).ToList();
            return Ok(response);
        }

        [HttpGet("GetBookingByIdUser")]
        public async Task<ActionResult<List<BookingResponse>>> GetByUserId(Guid userId)
        {
            var booking = await _bookingService.GetBookingByUser(userId);

            var response = booking.Select(x => new BookingResponse(x)).ToList();

            return Ok(response);
        }

        //[HttpDelete("{bookingId}")]
        /// <summary>
        /// Its method never uses because we have a cancellation booking
        /// </summary>
        /// <param name="bookingId"></param>
        /// <returns></returns>
        [HttpDelete("DeleteBookingByIdBooking")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBooking(Guid bookingId)
        {
            _logger.LogInformation("Delete Booking by Booking id: {bookingId}", bookingId);

            try
            {
                await _bookingService.DeleteBooking(bookingId);
                return NoContent(); // 204 No Content - успішне видалення
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Booking with id {BookingId} not found", bookingId);
                return NotFound(); // 404 Not Found - бронювання не знайдено
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking with id {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError); // 500 Internal Server Error
            }
        }

        [HttpPost("cancel-booking")]
        public async Task<IActionResult> CancellationBooking(Guid bookingId)
        {
            _logger.LogInformation("Change Booking state to cancel by Booking id: {BookingId}", bookingId);

            try
            {
                await _bookingService.CancellationBooking(bookingId);
                return NoContent(); // 204 No Content - успішно змінено статус
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Booking with id {BookingId} not found", bookingId);
                return NotFound(); // 404 Not Found
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking with id {BookingId}", bookingId);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message); // 500 Internal Server Error
            }
        }


        [HttpPost("bookings")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var bookingId = await _bookingService.CreateBookingAsync(request);
                return Ok(new { BookingId = bookingId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("bookings/guest")]
        public async Task<IActionResult> CreateGuestBooking([FromBody] CreateGuestBookingRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                _logger.LogWarning("ModelState invalid for guest booking: {Errors}", string.Join(", ", errors));
                return BadRequest(new { message = "Некоректні дані", errors });
            }

            try
            {
                var bookingId = await _bookingService.CreateGuestBookingAsync(request);
                return Ok(new { BookingId = bookingId, Message = "Бронювання гостя створено" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при створенні бронювання гостя");
                return BadRequest(new { message = ex.Message });
            }
        }

        // BookingController.cs
        // [HttpGet("available-slots/{sportsFieldId}/{date}/{sportType}")]
        // public async Task<ActionResult<List<TimeSlot>>> GetAvailableTimeSlots(
        //     Guid sportsFieldId, 
        //     DateTime date, 
        //     int sportType)
        // {
        //     try
        //     {
        //         var slots = await _bookingService.GetAvailableTimeSlots(sportsFieldId, date, sportType);
        //         return Ok(slots);
        //     }
        //     catch (Exception ex)
        //     {
        //         return BadRequest(ex.Message);
        //     }
        // }
        [HttpGet("available-slots/{sportsFieldId}/{date}/{sportType}/{instanceId?}")]
        public async Task<ActionResult<List<TimeSlot>>> GetAvailableTimeSlots(
            Guid sportsFieldId,
            string date,
            int sportType,
            Guid? instanceId = null)
        {
            try
            {
                if (!BookingsSportsFields.Core.UtcDateTimeHelper.TryParseIsoOrDateOnly(date, out var utcDay))
                    return BadRequest(
                        "Невірний формат дати. Очікується UTC-календарний день: 2026-04-07 або 2026-04-07T00:00:00.000Z");

                var slots = await _bookingService.GetAvailableTimeSlots(sportsFieldId, utcDay, sportType, instanceId);
                _logger.LogInformation("Повернуто {Count} слотів (UTC day {Day})", slots.Count, utcDay.ToString("yyyy-MM-dd"));
                return Ok(slots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка слотів");
                return BadRequest(ex.Message);
            }
        }

        
        [HttpDelete("cleanup-old-bookings")]
        public async Task<IActionResult> CleanupOldBookings()
        {
            try
            {
                var thresholdDate = DateTime.UtcNow.AddMonths(-1);
                await _bookingService.DeleteOldBookingsAsync(thresholdDate);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("check-availability")]
        public async Task<ActionResult<bool>> CheckAvailability([FromBody] CheckAvailabilityRequest request)
        {
            _logger.LogInformation(
                "CheckAvailability: Field={FieldId}, Instance={InstanceId}, Start={Start}, Duration={Duration}, Type={SportType}",
                request.SportsFieldId, request.SportsFieldInstanceId, request.StartTime, request.DurationMinutes, request.SportType
            );

            try
            {
                bool isAvailable = await _bookingService.CheckAvailability(
                    request.SportsFieldId,
                    request.StartTime,
                    request.DurationMinutes,
                    request.SportType,
                    request.SportsFieldInstanceId
                );

                _logger.LogInformation("Результат: {Result}", isAvailable);
                return Ok(isAvailable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка перевірки");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>Підтвердити одне бронювання (Pending → Confirmed). Лише власник майданчика.</summary>
        [Authorize]
        [HttpPost("manager/bookings/{bookingId:guid}/confirm")]
        public async Task<IActionResult> ConfirmBookingByManager(Guid bookingId)
        {
            var ownerId = ParseUserId(User);
            if (ownerId == null)
                return Unauthorized();

            try
            {
                await _bookingService.ConfirmBookingByManagerAsync(bookingId, ownerId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Скасувати бронювання менеджером (не пізніше ніж за 1 год до початку).</summary>
        [Authorize]
        [HttpPost("manager/bookings/{bookingId:guid}/cancel")]
        public async Task<IActionResult> CancelBookingByManager(Guid bookingId)
        {
            var ownerId = ParseUserId(User);
            if (ownerId == null)
                return Unauthorized();

            try
            {
                await _bookingService.CancelBookingByManagerAsync(bookingId, ownerId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Підтвердити всі Pending на майданчику.</summary>
        [Authorize]
        [HttpPost("manager/fields/{sportsFieldId:guid}/confirm-all-pending")]
        public async Task<ActionResult<ConfirmAllPendingResponse>> ConfirmAllPendingForField(Guid sportsFieldId)
        {
            var ownerId = ParseUserId(User);
            if (ownerId == null)
                return Unauthorized();

            try
            {
                var count = await _bookingService.ConfirmAllPendingForFieldAsync(sportsFieldId, ownerId.Value);
                return Ok(new ConfirmAllPendingResponse { ConfirmedCount = count });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        /// <summary>Автопідтвердження нових бронювань для майданчика.</summary>
        [Authorize]
        [HttpPut("manager/fields/{sportsFieldId:guid}/auto-confirm")]
        public async Task<IActionResult> SetAutoConfirmForField(Guid sportsFieldId, [FromBody] AutoConfirmBookingsRequest? body)
        {
            var ownerId = ParseUserId(User);
            if (ownerId == null)
                return Unauthorized();
            if (body == null)
                return BadRequest(new { message = "Очікується тіло { \"enabled\": true|false }" });

            try
            {
                await _bookingService.SetAutoConfirmForFieldAsync(sportsFieldId, body.Enabled, ownerId.Value);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        private static Guid? ParseUserId(ClaimsPrincipal? user)
        {
            if (user == null)
                return null;
            var s = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(s, out var id) ? id : null;
        }

        // [Authorize]
        [AllowAnonymous]
        [HttpGet("filtered-bookings-crm")]
        public async Task<ActionResult<List<BookingsEntity>>> GetFilteredBookingsCRM([FromQuery] Guid ownerId,[FromQuery] int? status, [FromQuery] DateTime? date, [FromQuery] string? titleOfSportFild)
        {
            // if (ownerId == null)
            // {
            //     _logger.LogWarning("Unauthorized access attempt to GetFiltered bookings.");
            //     return Unauthorized();
            // }
            
            _logger.LogInformation("GetFiltered bookings with ownerId: {ownerId}", ownerId);
            var bookings = await _bookingService.GetReservedReservationsForFieldOwnerCRM(ownerId, status, date, titleOfSportFild);
            
            return Ok(bookings);
        }
    }
    
    
    
    public class CheckAvailabilityRequest
    {
        public Guid SportsFieldId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public int SportType { get; set; }
        public Guid? SportsFieldInstanceId { get; set; }  // ← ДОДАЙ ЦЕ ПОЛЕ
    }

    public class AutoConfirmBookingsRequest
    {
        /// <summary>Якщо true — нові бронювання одразу Confirmed.</summary>
        public bool Enabled { get; set; }
    }

    public class ConfirmAllPendingResponse
    {
        public int ConfirmedCount { get; set; }
    }
}
