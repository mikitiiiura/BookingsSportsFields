using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.Application.Contracts.Response;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.Core.Model;
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

        [HttpGet("GetBookingByIdUser")]
        public async Task<ActionResult<List<BookingResponse>>> GetByUserId(Guid userId)
        {
            var booking = await _bookingService.GetBookingByUser(userId);

            var response = booking.Select(x => new BookingResponse(x)).ToList();

            return Ok(response);
        }

        //[HttpDelete("{bookingId}")]
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
        [HttpGet("available-slots/{sportsFieldId}/{date}")]
        public async Task<ActionResult<List<TimeSlot>>> GetAvailableTimeSlots(
    Guid sportsFieldId,
    DateTime date)
        {
            try
            {
                var slots = await _bookingService.GetAvailableTimeSlots(sportsFieldId, date);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("check-availability")]
        public async Task<ActionResult<bool>> CheckAvailability(
            [FromBody] CheckAvailabilityRequest request)
        {
            try
            {
                bool isAvailable = await _bookingService.CheckAvailability(
                    request.SportsFieldId,
                    request.StartTime,
                    request.DurationMinutes);

                return Ok(isAvailable);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
    public class CheckAvailabilityRequest
    {
        public Guid SportsFieldId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
    }
}
