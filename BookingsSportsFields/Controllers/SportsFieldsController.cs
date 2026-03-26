using BookingsSportsFields.Application.Contracts.Request;
using BookingsSportsFields.Application.Contracts.Response;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.DataAccess.ModelEntity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using BookingsSportsFields.Core.Model;

namespace BookingsSportsFields.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SportsFieldsController : ControllerBase
    {
        private readonly ILogger<SportsFieldsController> _logger;
        private readonly ISportFildService _sportFildService;

        public SportsFieldsController(ILogger<SportsFieldsController> logger, ISportFildService sportFildService)
        {
            _logger = logger;
            _sportFildService = sportFildService;
        }

        [HttpGet]
        public async Task<ActionResult<List<SportsFieldResponce>>> GetAllSportFild()
        {
            var sportFields = await _sportFildService.GetAll();
            if (sportFields == null || !sportFields.Any())
            {
                return NotFound();
            }

            var response = sportFields.Select(x => new SportsFieldResponce(x)).ToList();
            return Ok(response);
        }

        [HttpGet("GetAllSportFieldByOwnerID")]
        public async Task<ActionResult<List<SportsFieldByUser>>> GetAllSportFildByOwnerID(Guid ownerId)
        {
            var sportFields = await _sportFildService.GetAllByOwnerID(ownerId);
            if (sportFields == null || !sportFields.Any())
            {
                return NotFound();
            }

            var response = sportFields.Select(x => new SportsFieldByUser(x)).ToList();
            return Ok(response);
        }

        [HttpGet("FilteredSportFild")]
        public async Task<ActionResult<List<SportsFieldResponce>>> GetFilteredSportFild(
            int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration, string? city)
        {
            var sportfild = await _sportFildService.GetFilteredFild(type, searchTitleOrAddres, date, startTime, duration, city);
            if (sportfild == null || !sportfild.Any())
            {
                return NotFound();
            }
            var responce = sportfild.Select(x => new SportsFieldResponce(x)).ToList();
            return Ok(responce);
        }

        [HttpPost("AddSportFild")]
        public async Task<IActionResult> CreateSportsField([FromBody] CreateSportsFieldDto dto)
        {
            try
            {
                var sportsFild = new SportsFieldsEntity
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    ImageUrl = dto.ImageUrl,
                    OwnerId = dto.OwnerId,
                    CreatedAt = DateTime.UtcNow,
                    Location = new LocationsEntity
                    {
                        Id = Guid.NewGuid(),
                        Address = dto.Location.Address,
                        City = dto.Location.City,
                        Latitude = dto.Location.Latitude,
                        Longitude = dto.Location.Longitude,
                    }
                };

                sportsFild.TypesWithDetails = dto.Types.Select(typeDto =>
                {
                    var typeId = Guid.NewGuid();

                    var typeEntity = new SportsFieldSportTypeEntity
                    {
                        Id = typeId,
                        Type = typeDto.Type,
                        PricePerHour = typeDto.PricePerHour,
                        WarningInformation = typeDto.WarningInformation,
                        SportsFieldId = sportsFild.Id,
                        WeeklySchedules = typeDto.WeeklySchedules.Select(ws => new SportsFieldSchedule
                        {
                            Id = Guid.NewGuid(),
                            DayOfWeek = ws.DayOfWeek,
                            AvailableFrom = ws.AvailableFrom,
                            AvailableTo = ws.AvailableTo,
                            SportsFieldSportTypeId = typeId
                        }).ToList()
                    };

                    var instances = new List<SportsFieldInstanceEntity>();
                    int qty = typeDto.Quantity > 0 ? typeDto.Quantity : 1;

                    if (typeDto.Instances != null && typeDto.Instances.Any())
                    {
                        foreach (var inst in typeDto.Instances)
                        {
                            instances.Add(new SportsFieldInstanceEntity
                            {
                                Id = Guid.NewGuid(),
                                DisplayName = inst.DisplayName,
                                SportTypeId = typeId,
                                IsActive = true
                            });
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= qty; i++)
                        {
                            instances.Add(new SportsFieldInstanceEntity
                            {
                                Id = Guid.NewGuid(),
                                DisplayName = $"{typeDto.Type} №{i}",
                                SportTypeId = typeId,
                                IsActive = true
                            });
                        }
                    }

                    typeEntity.Instances = instances;
                    return typeEntity;
                }).ToList();

                sportsFild.Location.SportsFieldId = sportsFild.Id;

                await _sportFildService.AddSportsFields(sportsFild);

                return Ok(new { Message = "Спортивний майданчик створено", Id = sportsFild.Id });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Помилка при створенні спортивного майданчика");
                return StatusCode(500, "Виникла внутрішня помилка при створенні майданчика");
            }
        }

        // Новий ендпоінт для інстансів — ВИПРАВЛЕНО
        [HttpGet("{fieldId}/types/{sportType}/instances")]
        public async Task<ActionResult<List<SportsFieldInstanceDto>>> GetInstancesForType(Guid fieldId, int sportType)
        {
            // Отримуємо майданчик через сервіс
            var field = await _sportFildService.GetByIdWithDetailsAsync(fieldId);
            if (field == null) return NotFound("Майданчик не знайдено");

            var type = field.TypesWithDetails.FirstOrDefault(t => (int)t.Type == sportType);
            if (type == null) return NotFound("Тип спорту не знайдено на цьому майданчику");

            var instances = type.Instances
                .Where(i => i.IsActive)
                .Select(i => new SportsFieldInstanceDto(i.Id, i.DisplayName))
                .ToList();

            return Ok(instances);
        }

        // Record тепер з двома параметрами — використовуємо правильно
        public record SportsFieldInstanceDto(Guid Id, string DisplayName);
        
        
        [HttpPut("update-sport-fields/{id}")]
        public async Task<IActionResult> UpdateSportsField(Guid id, [FromBody] UpdateSportsFieldDto dto)
        {
            if (id != dto.Id) return BadRequest("ID не співпадає");

            try
            {
                await _sportFildService.UpdateAsync(dto);
                return Ok(new { Message = "Майданчик оновлено", Id = dto.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка оновлення майданчика {Id}. DTO: {@Dto}", id, dto);
                return StatusCode(500, new { Message = "Внутрішня помилка", Error = ex.Message, Inner = ex.InnerException?.Message });
            }
        }
        // [HttpPut("update-sport-fields/{id}")]
        // public async Task<IActionResult> UpdateSportsField(Guid id, [FromBody] UpdateSportsFieldDto dto)
        // {
        //     if (id != dto.Id) return BadRequest("ID не співпадає");
        //     try
        //     {
        //         await _sportFildService.UpdateAsync(dto);
        //         return Ok(new { Message = "Майданчик оновлено", Id = dto.Id });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Помилка оновлення {Id}", id);
        //         return StatusCode(500, "Внутрішня помилка");
        //     }
        // }

        [HttpPut("update-sport-fields/{id}/image")]
        public async Task<IActionResult> UpdateImage(Guid id, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("Файл не завантажено");

            try
            {
                var newImageUrl = await _sportFildService.UpdateSportsFieldImageAsync(id, imageFile);
                return Ok(new { imageUrl = newImageUrl });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Майданчик не знайдено");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка завантаження зображення для майданчика {Id}", id);
                return StatusCode(500, "Помилка при збереженні зображення");
            }
        }

        [HttpDelete("delete-sport-fields/{id}")]
        public async Task<IActionResult> DeleteSportsField(Guid id)
        {
            var isDeleted = await _sportFildService.DeleteAsync(id);

            if (!isDeleted)
            {
                return NotFound(new { Message = "Майданчик не знайдено або він уже видалений" });
            }

            return Ok(new { Message = "Успішно видалено майданчик" });
        }

        // DTO-класи без змін (тут все ок)
        public class CreateSportsFieldDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public CreateLocationDto Location { get; set; } = new();
            public Guid? OwnerId { get; set; }
            public List<CreateSportTypeDetailDto> Types { get; set; } = new();
        }

        public class CreateLocationDto
        {
            public string Address { get; set; }
            public string City { get; set; }
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
        }

        public class CreateSportTypeDetailDto
        {
            public SportFieldsType Type { get; set; }
            public double PricePerHour { get; set; }
            public string? WarningInformation { get; set; }
            public List<CreateWeeklyScheduleDto> WeeklySchedules { get; set; } = new();
            public int Quantity { get; set; } = 1;
            public List<CreateInstanceDto> Instances { get; set; } = new();
        }

        public class CreateInstanceDto
        {
            public string DisplayName { get; set; } = string.Empty;
        }

        public class CreateWeeklyScheduleDto
        {
            public DayOfWeek DayOfWeek { get; set; }
            public TimeSpan AvailableFrom { get; set; }
            public TimeSpan AvailableTo { get; set; }
        }

        public record FilterModel
        {
            [Range(0, 6, ErrorMessage = "Тип спорту повинен бути від 0 до 6.")]
            public int? type { get; init; }

            public string? searchTitleOrAddres { get; init; }

            [DataType(DataType.Date)]
            public DateTime? date { get; init; }

            public string? startTime { get; init; }

            public string? duration { get; init; }

            public string? city { get; init; }
        }
    }
}