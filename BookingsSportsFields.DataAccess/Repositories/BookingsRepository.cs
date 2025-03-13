using System;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace BookingsSportsFields.DataAccess.Repositories
{
    public class BookingsRepository
    {
        private readonly BookingsSportsFieldsDBContext _dBContext;
        private readonly ILogger<BookingsRepository> _logger;

        public BookingsRepository(BookingsSportsFieldsDBContext dBContext, ILogger<BookingsRepository> logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }

        public async Task<List<BookingsEntity>> GetAll()
        {
            return await _dBContext.Bookings
                .Include(b => b.User)
                .Include(b => b.SportsField)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<BookingsEntity>> GetAllByID(Guid userId)
        {
            return await _dBContext.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.User)
                .Include(b => b.SportsField)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
