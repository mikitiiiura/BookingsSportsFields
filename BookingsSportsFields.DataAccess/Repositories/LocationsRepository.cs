using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace BookingsSportsFields.DataAccess.Repositories
{
    public class LocationsRepository
    {
        private readonly BookingsSportsFieldsDBContext _dBContext;
        private readonly ILogger _logger;

        public LocationsRepository(BookingsSportsFieldsDBContext dBContext, ILogger logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }
        public async Task<List<LocationsEntity>> GetAll()
        {
            _logger.LogInformation("Fetching all locations");

            return await _dBContext.Locations
                .Include(s => s.SportsFields)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<LocationsEntity>> GetAllByID(Guid sportFildId)
        {
            _logger.LogInformation("Fetching location with Sport Fild ID: {SportFildId}", sportFildId);
            return await _dBContext.Locations
                .Where(l => l.SportsFieldId == sportFildId)
                .Include(s => s.SportsFields)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
