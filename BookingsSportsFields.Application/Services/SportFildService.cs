using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookingsSportsFields.Application.Contracts.Request;

namespace BookingsSportsFields.Application.Services
{
    public class SportFildService : ISportFildService
    {
        private readonly ISportsFieldsRepository _sportsFieldsRepository;
        private readonly ILogger _logger;

        public SportFildService(ISportsFieldsRepository sportsFieldsRepository, ILogger<SportFildService> logger)
        {
            _sportsFieldsRepository = sportsFieldsRepository;
            _logger = logger;
        }

        // Отримати всі завдання
        public async Task<List<SportsFieldsEntity>> GetAll()
        {
            return await _sportsFieldsRepository.GetAll();
        }

        public async Task<List<SportsFieldsEntity>> GetAllByOwnerID(Guid ownerId)
        {
            return await _sportsFieldsRepository.GetAllByOwnerID(ownerId);
        }

        public async Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres,
            DateTime? date, string? startTime, string? duration, string? city)
        {
            return await _sportsFieldsRepository.GetFilteredFild(type, searchTitleOrAddres, date, startTime, duration,
                city);
        }

        public async Task<SportsFieldsEntity> AddSportsFields(SportsFieldsEntity sportsFields)
        {
            return await _sportsFieldsRepository.CreateSportsField(sportsFields);
        }



        public async Task UpdateAsync(UpdateSportsFieldDto dto)
        {
            _logger.LogInformation("Оновлення майданчика ID: {Id}", dto.Id);

            var existing = await _sportsFieldsRepository.GetByIdWithDetailsAsync(dto.Id);
            if (existing == null)
                throw new KeyNotFoundException($"SportsField {dto.Id} not found");

            if (dto.Name != null) existing.Name = dto.Name;
            if (dto.Description != null) existing.Description = dto.Description;
            if (dto.ImageUrl != null) existing.ImageUrl = dto.ImageUrl;

            if (dto.Types != null)
            {
                var newTypeEntities = dto.Types.Select(t => new SportsFieldSportTypeEntity
                {
                    Id = Guid.NewGuid(),
                    Type = t.Type,
                    PricePerHour = t.PricePerHour,
                    WarningInformation = t.WarningInformation ?? "",
                    WeeklySchedules = t.WeeklySchedules.Select(ws => new SportsFieldSchedule
                    {
                        Id = Guid.NewGuid(),
                        DayOfWeek = ws.DayOfWeek,
                        AvailableFrom = ws.AvailableFrom,
                        AvailableTo = ws.AvailableTo
                    }).ToList()
                }).ToList();

                await _sportsFieldsRepository.ReplaceTypesAndSchedulesAsync(existing.Id, newTypeEntities);
            }

            await _sportsFieldsRepository.SaveChangesAsync();
        }
    }
}

