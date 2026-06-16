namespace Palantir.Presenatation.Controllers
{

    [ApiController]
    [Route("api/army")]
    public class ArmiesController : ControllerBase
    {
        private readonly IArmyService _armyService;

        public ArmiesController(IArmyService armyService) =>
            _armyService = armyService;


        [HttpGet("{id}")]
        public async Task<ActionResult<ArmyResponse>> GetById(int id)
        {
            var army = await _armyService.GetByIdAsync(id);
            return army is null ? 
                NotFound() : Ok(army);
        }

        [HttpGet("by-war/{warId:int}/positions")]
        public async Task<ActionResult<List<ArmyMapResponse>>>
            GetByWarDateWithPositions(int warId, [FromQuery] DateOnly date)
        {
            var armies = await _armyService
                .GetByWarDateWithPositionsAsync(warId, date);

            return Ok(armies);
        }

        [HttpPost]
        public async Task<ActionResult> Add(CreateArmyRequest armyRequest)
        {
            var response = await _armyService.AddAsync(armyRequest);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPost("with-position")]
        public async Task<ActionResult> AddWithPosition(CreateArmyWithPositionRequest armyRequest)
        {
            var response = await _armyService.AddWithPositionAsync(armyRequest);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id:int}/position")]
        public async Task<ActionResult<ArmyMapResponse>> UpdatePosition(
            int id,
            UpdateArmyMapPositionRequest positionRequest)
        {
            var response = await _armyService
                .UpdatePositionAsync(id, positionRequest);

            return response is null
                ? NotFound()
                : Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateArmyRequest armyRequest)
        {
            var response = await _armyService
                .UpdateAsync(id, armyRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteByRoute(int id)
        {
            await _armyService.DeleteAsync(id);
            return NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            await _armyService.DeleteAsync(id);
            return NoContent();
        }
    }
}
