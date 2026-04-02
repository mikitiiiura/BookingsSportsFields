using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.Application.Contracts.Response;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.Extensions.Logging;

namespace BookingsSportsFields.Application.Services;

public class ReviewService : IReviewService
{
    private readonly ISportsFieldsRepository _sportsFieldsRepo;
    private readonly IBookingsRepository _bookingsRepository;
    private readonly IReviewsRepository _reviewsRepo;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(
        ISportsFieldsRepository sportsFieldsRepo,
        IBookingsRepository bookingsRepository,
        IReviewsRepository reviewsRepo,
        ILogger<ReviewService> logger)
    {
        _sportsFieldsRepo = sportsFieldsRepo ?? throw new ArgumentNullException(nameof(sportsFieldsRepo));
        _bookingsRepository = bookingsRepository ?? throw new ArgumentNullException(nameof(bookingsRepository));
        _reviewsRepo = reviewsRepo ?? throw new ArgumentNullException(nameof(reviewsRepo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> AddReviewAsync(CreateReviewRequest request, Guid userId)
    {
        _logger.LogInformation("=== Додавання відгуку === UserId={UserId}, SportsFieldId={FieldId}, BookingId={BookingId}", 
            userId, request.SportsFieldId, request.BookingId);

        // Перевірка, чи користувач бронював майданчик
        bool hasBooked = await _bookingsRepository.UserHasCompletedBookingAsync(userId, request.SportsFieldId);
        if (!hasBooked)
        {
            _logger.LogWarning("Користувач не має завершеного бронювання");
            throw new UnauthorizedAccessException("Ви можете залишити відгук тільки після завершеного бронювання цього майданчика.");
        }

        // Перевірка, чи вже є відгук на це бронювання
        if (request.BookingId.HasValue)
        {
            bool alreadyReviewed = await _reviewsRepo.HasReviewForBookingAsync(request.BookingId.Value, userId);
            if (alreadyReviewed)
            {
                _logger.LogWarning("Відгук на це бронювання вже існує");
                throw new InvalidOperationException("Ви вже залишили відгук на це бронювання.");
            }
        }

        var review = new ReviewsEntity
        {
            Id = Guid.NewGuid(),
            SportsFieldId = request.SportsFieldId,
            UserId = userId,
            BookingId = request.BookingId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim() ?? "",
            CreatedAt = DateTime.UtcNow
        };

        await _sportsFieldsRepo.AddReviewAsync(review);

        _logger.LogInformation("Відгук успішно збережено. ReviewId = {ReviewId}", review.Id);

        return review.Id;
    }

    // Інші методи залишаємо без змін
    public async Task<List<ReviewResponse>> GetReviewsForFieldAsync(Guid sportsFieldId)
    {
        var reviews = await _sportsFieldsRepo.GetReviewsBySportsFieldAsync(sportsFieldId);
        return reviews.Select(r => new ReviewResponse
        {
            Id = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt,
            UserName = r.User?.FullName ?? "Анонім"
        }).ToList();
    }

    public async Task<double> GetAverageRatingAsync(Guid sportsFieldId)
    {
        return await _sportsFieldsRepo.GetAverageRatingAsync(sportsFieldId);
    }
}