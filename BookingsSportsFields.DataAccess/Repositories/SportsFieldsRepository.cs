using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.Abstruction;
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
    public class SportsFieldsRepository : ISportsFieldsRepository
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
                .Include(sf => sf.TypesWithDetails)
                .ThenInclude(t => t.WeeklySchedules)
                .Include(sf => sf.Owner) // Навігаційна властивість
                .Include(sf => sf.Location) // Навігаційна властивість
                //.Include(sf => sf.Images) // Якщо потрібно отримати зображення майданчика
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
                .Include(sf => sf.TypesWithDetails)
                .ThenInclude(t => t.WeeklySchedules)
                .Include(sf => sf.Owner) // Навігаційна властивість
                                         //.Include(sf => sf.Images) // Якщо потрібно отримати зображення майданчика
                .AsNoTracking()
                .ToListAsync();
        }
        // /// <summary>
        // /// Add SportsFields
        // /// </summary>
        // /// <param name="sportsFields"></param>
        // /// <returns></returns>
        // public async Task Add(SportsFieldsEntity sportsFields)
        // {
        //     await _dBContext.SportsFields.AddAsync(sportsFields);
        //     await _dBContext.SaveChangesAsync();
        // }

        /// <summary>
        /// !NOT WORK! Update SportsFields !NOT WORK!
        /// </summary>
        /// <param name="sportsFilds"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<SportsFieldsEntity?> GetByIdAsync(Guid id)
        {
            return await _dBContext.SportsFields
                .Include(sf => sf.TypesWithDetails)
                .ThenInclude(t => t.WeeklySchedules)
                .Include(sf => sf.Location)
                .Include(sf => sf.Owner)
                .FirstOrDefaultAsync(sf => sf.Id == id);
        }

        public async Task DeleteTypesForFieldAsync(Guid sportsFieldId)
        {
            var typesToDelete = await _dBContext.SportsFieldSportTypes
                .Where(t => t.SportsFieldId == sportsFieldId)
                .ToListAsync();

            if (typesToDelete.Any())
            {
                _dBContext.SportsFieldSportTypes.RemoveRange(typesToDelete);
                await _dBContext.SaveChangesAsync();
            }
        }

        // public async Task UpdateAsync(SportsFieldsEntity entity)
        // {
        //     _dBContext.SportsFields.Update(entity);
        //     await _dBContext.SaveChangesAsync();
        //     }
        
       public async Task UpdateAsync(SportsFieldsEntity entity)
{
    _logger.LogInformation("Оновлення майданчика в репозиторії ID: {Id}", entity.Id);

    try
    {
        // Отримуємо свіжий стан з БД (щоб уникнути кешу EF з старими Id)
        var existing = await _dBContext.SportsFields
            .Include(sf => sf.TypesWithDetails)
                .ThenInclude(t => t.WeeklySchedules)
            .FirstOrDefaultAsync(sf => sf.Id == entity.Id);

        if (existing == null)
        {
            _logger.LogWarning("Майданчик не знайдено: {Id}", entity.Id);
            throw new KeyNotFoundException($"SportsField {entity.Id} not found");
        }

        // Оновлюємо базові поля
        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.ImageUrl = entity.ImageUrl;
        // CreatedAt, OwnerId, Location — НЕ чіпаємо

        // Повна заміна типів та розкладів
        if (entity.TypesWithDetails != null && entity.TypesWithDetails.Any())
        {
            // Видаляємо старі типи (каскадно видаляться розклади)
            _dBContext.SportsFieldSportTypes.RemoveRange(existing.TypesWithDetails);

            // Очищаємо колекцію в пам'яті
            existing.TypesWithDetails.Clear();

            // Додаємо нові типи з новими Id (EF зробить INSERT)
            foreach (var newType in entity.TypesWithDetails)
            {
                var typeEntity = new SportsFieldSportTypeEntity
                {
                    Id = Guid.NewGuid(),
                    Type = newType.Type,
                    PricePerHour = newType.PricePerHour,
                    WarningInformation = newType.WarningInformation ?? "",
                    WeeklySchedules = newType.WeeklySchedules.Select(ws => new SportsFieldSchedule
                    {
                        Id = Guid.NewGuid(),
                        DayOfWeek = ws.DayOfWeek,
                        AvailableFrom = ws.AvailableFrom,
                        AvailableTo = ws.AvailableTo
                    }).ToList()
                };

                existing.TypesWithDetails.Add(typeEntity);
            }
        }

        // Оновлюємо сутність (EF зробить UPDATE базових + INSERT нових типів/розкладів)
        _dBContext.SportsFields.Update(existing);
        await _dBContext.SaveChangesAsync();

        _logger.LogInformation("Оновлення успішно завершено для ID: {Id}", entity.Id);
    }
    catch (DbUpdateConcurrencyException ex)
    {
        _logger.LogError(ex, "Concurrency помилка при оновленні майданчика {Id}", entity.Id);
        throw new Exception("Конфлікт даних — майданчик міг бути змінений іншим користувачем", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Помилка оновлення майданчика {Id}: {Message}", entity.Id, ex.Message);
        throw;
    }
}
       
// Новий метод — оновлює ТІЛЬКИ ImageUrl, не чіпає типи
public async Task UpdateImageUrlAsync(Guid id, string newImageUrl)
{
    var entity = await _dBContext.SportsFields
        .FirstOrDefaultAsync(sf => sf.Id == id);

    if (entity == null)
        throw new KeyNotFoundException($"SportsField {id} not found");

    entity.ImageUrl = newImageUrl;

    _dBContext.SportsFields.Update(entity);
    await _dBContext.SaveChangesAsync();

    _logger.LogInformation("ImageUrl оновлено для майданчика {Id}: {NewUrl}", id, newImageUrl);
}
       
       
public async Task<SportsFieldsEntity?> GetByIdWithDetailsAsync(Guid id)
{
    return await _dBContext.SportsFields
        .Include(sf => sf.TypesWithDetails)
        .ThenInclude(t => t.WeeklySchedules)
        .FirstOrDefaultAsync(sf => sf.Id == id);
}

public async Task ReplaceTypesAndSchedulesAsync(Guid sportsFieldId, List<SportsFieldSportTypeEntity> newTypes)
{
    // Видаляємо старі типи (каскадно видаляться розклади)
    var oldTypes = await _dBContext.SportsFieldSportTypes
        .Where(t => t.SportsFieldId == sportsFieldId)
        .ToListAsync();

    _dBContext.SportsFieldSportTypes.RemoveRange(oldTypes);

    // Додаємо нові типи
    foreach (var type in newTypes)
    {
        type.SportsFieldId = sportsFieldId;     // ← ОБОВ'ЯЗКОВО!
        _dBContext.SportsFieldSportTypes.Add(type);
    }
}

public async Task Delete(Guid id)
{
    await _dBContext.SportsFields.Where(sf => sf.Id == id).ExecuteDeleteAsync();
}

public async Task SaveChangesAsync()
{
    await _dBContext.SaveChangesAsync();
}
        // public async Task Update(SportsFieldsEntity sportsFilds)
        // {
        //     _logger.LogInformation("Updating SportsFilds with ID: {SportsFildsId}", sportsFilds.Id);
        //     var existingSportsFilds = await _dBContext.SportsFields
        //         .Include(sf => sf.TypesWithDetails)
        //         .ThenInclude(t => t.WeeklySchedules)
        //         .FirstOrDefaultAsync(sf => sf.Id == sportsFilds.Id);
        //
        //
        //     if (existingSportsFilds == null)
        //     {
        //         _logger.LogWarning("SportsFilds with ID {SportsFildsId} not found", sportsFilds.Id);
        //         throw new Exception("SportsFilds not found");
        //     }
        //
        //     existingSportsFilds.Name = sportsFilds.Name;
        //     // existingSportsFilds.Type = sportsFilds.Type;
        //     // existingSportsFilds.PricePerHour = sportsFilds.PricePerHour;
        //     existingSportsFilds.Description = sportsFilds.Description;
        //     existingSportsFilds.CreatedAt = sportsFilds.CreatedAt;
        //     //existingSportsFilds.LocationId = sportsFilds.LocationId;  //Тут потрібно додати перевірку і якимось чином додавати 
        //                                                               //зображення цих же спортивних майданчиків
        //     existingSportsFilds.OwnerId = sportsFilds.OwnerId;
        //     //existingSportsFilds.Images = sportsFilds.Images;//не знаю чи це норм
        //
        //     _dBContext.SportsFields.Update(existingSportsFilds);
        //     await _dBContext.SaveChangesAsync();
        // }

        /// <summary>
        /// Filtered Fild
        /// </summary>
        /// <param name="search"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration, string? city)
        {
            _logger.LogInformation("Fetching filtered sport fields");
            try
            {
                var query = _dBContext.SportsFields
                    .Include(sf => sf.TypesWithDetails)
                    .ThenInclude(t => t.WeeklySchedules)
                    .Include(s => s.Location)
                    .Include(s => s.Owner)
                    .Include(s => s.Bookings)
                    .AsNoTracking()
                    .AsQueryable();

                if (type.HasValue)
                {
                    //query = query.Where(s => (int)s.Type == type.Value);
                    query = query.Where(s => s.TypesWithDetails.Any(t => (int)t.Type == type.Value));
                }

                if (!string.IsNullOrEmpty(searchTitleOrAddres))
                {
                    query = query.Where(s =>
                        EF.Functions.Like(s.Name, $"%{searchTitleOrAddres}%") ||
                        EF.Functions.Like(s.Location.Address, $"%{searchTitleOrAddres}%"));
                }

                if (!string.IsNullOrEmpty(city))
                {
                    query = query.Where(s => s.Location.City.ToLower() == city.ToLower());
                }

                if (date.HasValue && !string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(duration))
                {
                    if (TimeSpan.TryParse(startTime, out var start) && double.TryParse(duration, out var durationHours))
                    {
                        var startDateTime = date.Value.Date + start;
                        var endDateTime = startDateTime.AddHours(durationHours);

                        query = query.Where(s =>
                            !s.Bookings.Any(b =>
                                b.StartTime < endDateTime &&
                                b.EndTime > startDateTime
                            )
                        );
                    }
                }

                var result = await query.ToListAsync();
                _logger.LogInformation("Successfully fetched filtered sport fields");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching filtered sport fields");
                throw;
            }
        }

        public async Task<SportsFieldsEntity> CreateSportsField(SportsFieldsEntity sportsFields)
        {
            _logger.LogInformation("Creating SportsField field");
            try
            {
                await _dBContext.SportsFields.AddAsync(sportsFields);
                await _dBContext.SaveChangesAsync();
                return sportsFields;
                
                // // 👉 Після збереження повторно отримуємо об'єкт з підключеним Location
                // var created = await _dBContext.SportsFields
                //     .Include(sf => sf.Location)
                //     .Include(sf => sf.TypesWithDetails)
                //     .ThenInclude(t => t.WeeklySchedules)
                //     .FirstOrDefaultAsync(sf => sf.Id == sportsFields.Id);
                //
                // return created!;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error creating SportsField");
                throw;
            }
        }

        


        //public async Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration)
        //{
        //    _logger.LogInformation("Fetching filtered sportFild");
        //    try
        //    {
        //        var query = _dBContext.SportsFields
        //            .Include(s => s.Location)
        //            .Include(s => s.Owner)
        //            //.Include(s => s.Images)
        //            .AsNoTracking()
        //            .AsQueryable();

        //        if (type.HasValue)
        //        {
        //            query = query.Where(s => (int)s.Type == type.Value);
        //        }

        //        if (!string.IsNullOrEmpty(searchTitleOrAddres))
        //        {
        //            query = query.Where(s => EF.Functions.Like(s.Name, $"%{searchTitleOrAddres}") || EF.Functions.Like(s.Location.Address, $"%{searchTitleOrAddres}"));
        //        }

        //        if(date.date)



        //        var sportFild = await query.ToListAsync();
        //        _logger.LogInformation("Successfully fetched filtered sportFild");
        //        return sportFild;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error occurred while fetching filtered sport Fild");
        //        throw;
        //    }
        //}
    }
}
