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

        [HttpGet("by-war-date")]
        public async Task<ActionResult<List<ControlZoneResponse>>>
            GetByWarDateQueryAsync([FromQuery] int warId, [FromQuery] DateOnly date)
        {
            var zones = await _zoneService.GetByWarDateAsync(warId, date);

            return Ok(zones);
        }

        [HttpGet("{warId:int}/{date}")]
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

        [HttpPut("{id:int}/geometry-for-date")]
        public async Task<ActionResult<ControlZoneGeometryForDateResponse>> UpdateGeometryForDate(
            int id,
            UpdateControlZoneGeometryForDateRequest request)
        {
            try
            {
                var response = await _zoneService.UpdateGeometryForDateAsync(id, request);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Зона контроля не найдена." });
            }
            catch (ArgumentException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _zoneService.DeleteAsync(id);
            return NoContent();
        }
    }
}
