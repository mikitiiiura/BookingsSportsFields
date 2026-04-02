using System.Security.Claims;
using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.Application.InterfaceServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(IReviewService reviewService, ILogger<ReviewController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] CreateReviewRequest request)
    {
        _logger.LogInformation("Отриманий запит на додавання відгуку: SportsFieldId={FieldId}, BookingId={BookingId}", 
            request.SportsFieldId, request.BookingId);

        if (request.UserId == Guid.Empty)
            return BadRequest(new { message = "UserId обов'язковий" });

        try
        {
            var reviewId = await _reviewService.AddReviewAsync(request, request.UserId);
            return Ok(new { message = "Відгук успішно додано", reviewId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при додаванні відгуку");
            return StatusCode(500, new { message = "Внутрішня помилка сервера" });
        }
    }

    [HttpGet("field/{sportsFieldId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetReviews(Guid sportsFieldId)
    {
        var reviews = await _reviewService.GetReviewsForFieldAsync(sportsFieldId);
        return Ok(reviews);
    }
}