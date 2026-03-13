

using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using BookingsSportsFields.Application.InterfaceServices;

namespace BookingsSportsFields.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Заповненість годин за конкретну дату (реальна vs можлива)
        /// </summary>
        [HttpGet("occupancy/{sportsFieldId}")]
        public async Task<IActionResult> GetOccupancy(
            Guid sportsFieldId,
            [FromQuery] DateTime date)
        {
            var result = await _analyticsService.GetOccupancyAsync(sportsFieldId, date);
            return Ok(result);
        }

        /// <summary>
        /// Статистика скасувань за період
        /// </summary>
        [HttpGet("cancellations/{sportsFieldId}")]
        public async Task<IActionResult> GetCancellations(
            Guid sportsFieldId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var result = await _analyticsService.GetCancellationStatsAsync(sportsFieldId, from, to);
            return Ok(result);
        }

        /// <summary>
        /// Прибуток за період (по днях)
        /// </summary>
        [HttpGet("revenue/{sportsFieldId}")]
        public async Task<IActionResult> GetRevenue(
            Guid sportsFieldId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var result = await _analyticsService.GetRevenueAsync(sportsFieldId, from, to);
            return Ok(result);
        }

        /// <summary>
        /// Середній рейтинг майданчика
        /// </summary>
        // [HttpGet("rating/{sportsFieldId}")]
        // public async Task<IActionResult> GetRating(Guid sportsFieldId)
        // {
        //     var result = await _analyticsService.GetAverageRatingAsync(sportsFieldId);
        //     return Ok(new { AverageRating = result });
        // }

        /// <summary>
        /// Найбільш заповнені години за період
        /// </summary>
        [HttpGet("peak-hours/{sportsFieldId}")]
        public async Task<IActionResult> GetPeakHours(
            Guid sportsFieldId,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var result = await _analyticsService.GetPeakHoursAsync(sportsFieldId, from, to);
            return Ok(result);
        }
    }
}