using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<List<SportsFieldsEntity>> GetFilteredFild(int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration)
        {
            return await _sportsFieldsRepository.GetFilteredFild(type, searchTitleOrAddres, date, startTime, duration);
        }
    }
}
