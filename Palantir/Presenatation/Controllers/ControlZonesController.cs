namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("api/control-zones")]
    public class ControlZonesController : ControllerBase
    {
        private readonly IControlZoneService _zoneService;

        public ControlZonesController(IControlZoneService controlZoneService) =>
            _zoneService = controlZoneService;


        [HttpGet("{id:int}")]
        public async Task<ActionResult<ControlZoneResponse>> GetById(int id)
        {
            var zone = await _zoneService.GetByIdAsync(id);
            return zone is null ?
                NotFound() : Ok(zone);
        }

        [HttpGet("{warId:int}/{date:datetime}")]
        public async Task<ActionResult<List<ControlZoneResponse>>>
            GetByWarDateAsync(int warId, DateOnly date)
        {
            var zones = await _zoneService.GetByWarDateAsync(warId, date);

            return Ok(zones);
        }

        [HttpPost]
        public async Task<ActionResult> Add(CreateControlZoneRequest zoneRequest)
        {
            var response = await _zoneService.AddAsync(zoneRequest);
            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response
            );
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, UpdateControlZoneRequest zoneRequest)
        {
            var response = await _zoneService
                .UpdateAsync(id, zoneRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _zoneService.DeleteAsync(id);
            return NoContent();
        }
    }
}
