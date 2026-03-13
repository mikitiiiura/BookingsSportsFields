using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BookingsSportsFields.Application.Services.Hosted_Service
{
    public class BookingStatusUpdater : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BookingStatusUpdater> _logger;

        public BookingStatusUpdater(IServiceProvider services, ILogger<BookingStatusUpdater> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервіс автоматичного завершення бронювань запущено");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<BookingsSportsFieldsDBContext>();

                    var now = DateTime.UtcNow;

                    // Знаходимо всі бронювання, які ще не завершені/не скасовані,
                    // і час закінчення +1 година минув
                    var expiredBookings = await db.Bookings
                        .Where(b => (b.Status == BookingStatus.Pending || 
                                     b.Status == BookingStatus.Confirmed) &&
                                    b.EndTime.AddHours(1) < now)
                        .ToListAsync(stoppingToken);

                    if (expiredBookings.Any())
                    {
                        foreach (var booking in expiredBookings)
                        {
                            var oldStatus = booking.Status;
                            booking.Status = BookingStatus.Completed;
                            _logger.LogInformation(
                                "Бронювання {Id} переведено з {OldStatus} в Completed (закінчення: {EndTime})",
                                booking.Id, oldStatus, booking.EndTime);
                        }

                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Автоматично завершено {Count} бронювань", expiredBookings.Count);
                    }
                    else
                    {
                        _logger.LogDebug("Немає бронювань для завершення на даний момент");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Помилка в сервісі завершення бронювань");
                }

                // Перевіряємо кожні 5 хвилин (можна зробити 10–15)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation("Сервіс завершення бронювань зупинено");
        }
    }
}