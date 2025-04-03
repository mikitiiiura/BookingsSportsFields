using BookingsSportsFields.Core.Model;
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
    public class SportsFieldsRepository
    {
        private readonly BookingsSportsFieldsDBContext _dBContext;
        private readonly ILogger<SportsFieldsRepository> _logger;

        public SportsFieldsRepository(BookingsSportsFieldsDBContext dBContext, ILogger<SportsFieldsRepository> logger)
        {
            _dBContext = dBContext;
            _logger = logger;
        }

        /// <summary>
        /// Get all SportsFields
        /// </summary>
        /// <returns></returns>
        public async Task<List<SportsFieldsEntity>> GetAll()
        {
            _logger.LogInformation("Fetching all sport field");
            return await _dBContext.SportsFields
                .Include(sf => sf.Location) // Навігаційна властивість
                .Include(sf => sf.Owner) // Навігаційна властивість
                .Include(sf => sf.Images) // Якщо потрібно отримати зображення майданчика
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Get All SportsFields By Owner ID
        /// </summary>
        /// <param name="ownerId"></param>
        /// <returns></returns>
        public async Task<List<SportsFieldsEntity>> GetAllByOwnerID(Guid ownerId)
        {
            return await _dBContext.SportsFields
                .Where(sp => sp.OwnerId == ownerId)
                .Include(sf => sf.Location) // Навігаційна властивість
                .Include(sf => sf.Owner) // Навігаційна властивість
                .Include(sf => sf.Images) // Якщо потрібно отримати зображення майданчика
                .AsNoTracking()
                .ToListAsync();
        }
        /// <summary>
        /// Add SportsFields
        /// </summary>
        /// <param name="sportsFields"></param>
        /// <returns></returns>
        public async Task Add(SportsFieldsEntity sportsFields)
        {
            await _dBContext.SportsFields.AddAsync(sportsFields);
            await _dBContext.SaveChangesAsync();
        }

        /// <summary>
        /// Update SportsFields
        /// </summary>
        /// <param name="sportsFilds"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task Update(SportsFieldsEntity sportsFilds)
        {
            _logger.LogInformation("Updating SportsFilds with ID: {SportsFildsId}", sportsFilds.Id);
            var existingSportsFilds = await _dBContext.SportsFields.FirstOrDefaultAsync(sf => sf.Id == sportsFilds.Id);

            if (existingSportsFilds == null)
            {
                _logger.LogWarning("SportsFilds with ID {SportsFildsId} not found", sportsFilds.Id);
                throw new Exception("SportsFilds not found");
            }

            existingSportsFilds.Name = sportsFilds.Name;
            existingSportsFilds.Type = sportsFilds.Type;
            existingSportsFilds.PricePerHour = sportsFilds.PricePerHour;
            existingSportsFilds.Description = sportsFilds.Description;
            existingSportsFilds.CreatedAt = sportsFilds.CreatedAt;
            existingSportsFilds.LocationId = sportsFilds.LocationId;  //Тут потрібнго додати перевірку і якимось чином додавати 
                                                                        //зображення цих же спортивних майданчиків
            existingSportsFilds.OwnerId = sportsFilds.OwnerId;
            //existingSportsFilds.Images = sportsFilds.Images;//незнаю чи це норм

            _dBContext.SportsFields.Update(existingSportsFilds);
            await _dBContext.SaveChangesAsync();
        }

        
        public async Task<List<SportsFieldsEntity>> GetFilteredFild(string? search, string? type ) 
        {
            _logger.LogInformation("Fetching filtered sportFild");
            try
            {
                var query = _dBContext.SportsFields
                    .Include(s => s.Location)
                    .Include(s => s.Owner)
                    .Include(s => s.Images)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(s => EF.Functions.Like(s.Name, $"%{search}") || EF.Functions.Like(s.Location.Address, $"%{search}")); 
                }

                if (!string.IsNullOrWhiteSpace(type))
                {
                    query = query.Where(s => (s.Type.ToString().ToLower()) == (type.ToLower()));
                }

                var sportFild = await query.ToListAsync();
                _logger.LogInformation("Successfully fetched filtered sportFild");
                return sportFild;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching filtered sport Fild");
                throw;
            }
        }
    }
}
