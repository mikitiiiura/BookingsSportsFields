using BookingsSportsFields.Application.Contracts.Response;
using BookingsSportsFields.Application.Contracts.Response.Analytics;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.Extensions.Logging;

public class AnalyticsService : IAnalyticsService
{
    private readonly IBookingsRepository _bookingsRepo;
    private readonly ISportsFieldsRepository _fieldsRepo;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IBookingsRepository bookingsRepo,
        ISportsFieldsRepository fieldsRepo,
        ILogger<AnalyticsService> logger)
    {
        _bookingsRepo = bookingsRepo;
        _fieldsRepo = fieldsRepo;
        _logger = logger;
    }

    public async Task<List<OccupancyDto>> GetOccupancyAsync(Guid sportsFieldId, DateTime date)
        {
            // Отримуємо розклад на цей день тижня
            var field = await _fieldsRepo.GetByIdWithDetailsAsync(sportsFieldId);
            if (field == null)
                throw new KeyNotFoundException($"Майданчик {sportsFieldId} не знайдено");

            var daySchedules = field.TypesWithDetails
                ?.SelectMany(t => t.WeeklySchedules)
                ?.Where(s => s.DayOfWeek == date.DayOfWeek)
                ?.ToList() ?? new List<SportsFieldSchedule>();

            // Отримуємо реальні бронювання на цю дату
            var bookings = await _bookingsRepo.GetBookingsForFieldByDateAsync(sportsFieldId, date);

            var result = new List<OccupancyDto>();

            // Проходимо по годинах від 0 до 23
            for (int h = 0; h < 24; h++)
            {
                var hourStart = new TimeSpan(h, 0, 0);
                var hourEnd = hourStart.Add(TimeSpan.FromHours(1));

                // Скільки слотів можливо (чи є розклад на цю годину)
                var possible = daySchedules.Any(s =>
                    s.AvailableFrom <= hourStart && s.AvailableTo >= hourEnd) ? 1 : 0;

                // Скільки реально заброньовано
                var booked = bookings.Count(b =>
                    b.StartTime.TimeOfDay <= hourStart && b.EndTime.TimeOfDay >= hourEnd);

                result.Add(new OccupancyDto
                {
                    Hour = $"{h:00}:00",
                    PossibleSlots = possible,
                    BookedSlots = booked,
                    OccupancyPercent = possible > 0 ? Math.Round((double)booked / possible * 100, 1) : 0
                });
            }

            return result;
        }

        public async Task<CancellationStatsDto> GetCancellationStatsAsync(Guid sportsFieldId, DateTime from, DateTime to)
        {
            var bookings = await _bookingsRepo.GetBookingsForFieldByPeriodAsync(sportsFieldId, from, to);

            var total = bookings.Count;
            var cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled);

            return new CancellationStatsDto
            {
                TotalBookings = total,
                Cancelled = cancelled,
                CancellationPercent = total > 0 ? Math.Round((double)cancelled / total * 100, 1) : 0
            };
        }

        public async Task<List<RevenueDto>> GetRevenueAsync(Guid sportsFieldId, DateTime from, DateTime to)
        {
            var bookings = await _bookingsRepo.GetBookingsForFieldByPeriodAsync(sportsFieldId, from, to);

            var grouped = bookings
                .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed ) // тільки успішні
                .GroupBy(b => b.StartTime.Date)
                .Select(g => new RevenueDto
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(b => b.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return grouped;
        }

        public async Task<RatingStatsDto> GetRatingStatsAsync(Guid sportsFieldId)
        {
            var avg = await _fieldsRepo.GetAverageRatingAsync(sportsFieldId);
            var count = await _fieldsRepo.GetReviewCountAsync(sportsFieldId);
            var reviews = await _fieldsRepo.GetReviewsBySportsFieldAsync(sportsFieldId);

            return new RatingStatsDto
            {
                AverageRating = avg,
                ReviewCount = count,
                Reviews = reviews.Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UserName = r.User?.FullName ?? "Анонім"
                }).ToList()
            };
        }

        public async Task<List<PeakHourDto>> GetPeakHoursAsync(Guid sportsFieldId, DateTime from, DateTime to)
        {
            var counts = await _bookingsRepo.GetHourlyBookingCountsAsync(sportsFieldId, from, to);

            var total = counts.Values.Sum();

            return counts
                .Select(kv => new PeakHourDto
                {
                    Hour = $"{kv.Key:00}:00",
                    BookingCount = kv.Value,
                    PercentOfTotal = total > 0 ? Math.Round((double)kv.Value / total * 100, 1) : 0
                })
                .OrderByDescending(x => x.BookingCount)
                .Take(5) // топ-5 годин
                .ToList();
        }
    }
