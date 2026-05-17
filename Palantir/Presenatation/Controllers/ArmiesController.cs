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

        [HttpPost]
        public async Task<ActionResult> Add(CreateArmyRequest armyRequest)
        {
            var response = await _armyService.AddAsync(armyRequest);
            return CreatedAtAction(nameof(GetById), new { id = response.Id });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateArmyRequest armyRequest)
        {
            var response = await _armyService
                .UpdateAsync(id, armyRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            await _armyService.DeleteAsync(id);
            return NoContent();
        }
    }
}
