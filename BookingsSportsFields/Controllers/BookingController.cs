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
                return BadRequest(ModelState);

            try
            {
                var bookingId = await _bookingService.CreateGuestBookingAsync(request);
                return Ok(new { BookingId = bookingId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
            DateTime date, 
            int sportType,
            Guid? instanceId = null)
        {
            try
            {
                var slots = await _bookingService.GetAvailableTimeSlots(sportsFieldId, date, sportType, instanceId);
                _logger.LogInformation("Повернуто {Count} слотів", slots.Count);
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
}
