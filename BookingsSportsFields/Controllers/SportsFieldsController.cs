using BookingsSportsFields.Application.Contracts.Response;
using BookingsSportsFields.Application.InterfaceServices;
using BookingsSportsFields.Core.Model;
using BookingsSportsFields.DataAccess.Abstruction;
using BookingsSportsFields.DataAccess.ModelEntity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace BookingsSportsFields.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
  
    public class SportsFieldsController : ControllerBase
    {
        //private readonly IMediator _mediator;
        private readonly ILogger<SportsFieldsController> _logger;
        private readonly ISportFildService _sportFildService;

        public SportsFieldsController(/*(IMediator mediator, */ILogger<SportsFieldsController> logger, ISportFildService sportFildService)
        {
            //_mediator = mediator;
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
        public async Task<ActionResult<List<SportsFieldResponce>>> GetAllSportFildByOwnerID(Guid ownerId)
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
        public async Task<ActionResult<List<SportsFieldResponce>>> GetFilteredSportFild(int? type, string? searchTitleOrAddres, DateTime? date, string? startTime, string? duration, string? city)
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
                // Спочатку створюємо локацію
                var location = new LocationsEntity
                {
                    Id = Guid.NewGuid(),
                    Address = dto.Location.Address,
                    City = dto.Location.City,
                    Latitude = dto.Location.Latitude,
                    Longitude = dto.Location.Longitude,
                    SportsFieldId = Guid.NewGuid() // Це буде оновлено після створення майданчика
                };
                
                var sportsFild = new SportsFieldsEntity
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name,
                    Description = dto.Description,
                    ImageUrl = dto.ImageUrl,
                    Location = location,
                    OwnerId = dto.OwnerId,
                    CreatedAt = DateTime.UtcNow,
                    TypesWithDetails = dto.Types.Select(typeDto =>
                    {
                        var typeId = Guid.NewGuid();
                        return new SportsFieldSportTypeEntity
                        {
                            Id = typeId,
                            Type = typeDto.Type,
                            PricePerHour = typeDto.PricePerHour,
                            WarningInformation = typeDto.WarningInformation,
                            WeeklySchedules = typeDto.WeeklySchedules.Select(ws => new SportsFieldSchedule
                            {
                                Id = Guid.NewGuid(),
                                DayOfWeek = ws.DayOfWeek,
                                AvailableFrom = ws.AvailableFrom,
                                AvailableTo = ws.AvailableTo,
                                SportsFieldSportTypeId = typeId
                            }).ToList()
                        };
                    }).ToList()
                };
                
                // Оновлюємо SportsFieldId в локації
                location.SportsFieldId = sportsFild.Id;
                await _sportFildService.AddSportsFields(sportsFild);
                
                //return Ok(sportsFild);
                
                return Ok(new { Message = "Спортивний майданчик створено", Id = sportsFild.Id });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Помилка при створенні спортивного майданчика");
                return StatusCode(500, "Виникла внутрішня помилка при створенні майданчика");
            }
        }
        
        public class CreateSportsFieldDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public CreateLocationDto  Location { get; set; }
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
