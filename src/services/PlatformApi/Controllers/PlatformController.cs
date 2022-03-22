using Microsoft.AspNetCore.Mvc;
using PlatformApi.Business.Abstractions;
using PlatformApi.Models.Dto;

namespace PlatformApi.Controllers
{
    [ApiController]
    [Route("api/[controller]s")]
    public class PlatformController : ControllerBase
    {
        private readonly ILogger<PlatformController> _logger;
        private readonly IPlatformService _platformService;

        public PlatformController(ILogger<PlatformController> logger, IPlatformService platformService)
        {
            _logger = logger;
            _platformService = platformService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PlatformReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            Console.WriteLine("--> Get All Called.");
            _logger.LogWarning("--> Get All Called.");
            var all =  await _platformService.GetAll();
            if (all == null) return NotFound();
            return Ok(all);
        }

        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType(typeof(PlatformReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var found = await _platformService.GetById(id);
            if(found == null) return NotFound();
            return Ok(found);
        }

        [HttpPost]
        [ProducesResponseType(typeof(PlatformReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(PlatformCreateDto dto)
        {
            var created = await _platformService.Create(dto);
            if(created == null) return NotFound();
            return CreatedAtRoute("GetById", new {id = created.Id} ,created);
        }
    }
}