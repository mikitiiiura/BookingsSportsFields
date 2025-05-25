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
