using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingsSportsFields.DataAccess.Repositories
{
    public class ReviewsRepository
    {
        private readonly BookingsSportsFieldsDBContext _dBContext;
        private readonly ILogger<ReviewsRepository> _logger;

        public ReviewsRepository(BookingsSportsFieldsDBContext dBContext, ILogger<ReviewsRepository> logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }

        public async Task<List<ReviewsEntity>> GetAll()
        {
            _logger.LogInformation("Fetching all reviews");
            return await _dBContext.Reviews
                .Include(b => b.User)

                .Include(b => b.SportsField)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
