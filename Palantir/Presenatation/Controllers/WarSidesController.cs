namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarSidesController : ControllerBase
    {
        private readonly IWarSideService _warSideService;

        public WarSidesController(IWarSideService warSideService) =>
            _warSideService = warSideService;


        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarSide>>> GetAll()
        {
            var warSides = await _warSideService.GetAllAsync();
            return Ok(warSides);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WarSide>> GetById(int id)
        {
            var warSide = await _warSideService.GetByIdAsync(id);
            return warSide is null ?
                NotFound() : Ok(warSide);
        }

        [HttpPost]
        public async Task<ActionResult> Add(CreateWarSideRequest warSideRequest)
        {
            await _warSideService.AddAsync(warSideRequest);
            return Ok();
        }

        [HttpPut]
        public async Task<ActionResult> Update(int id, UpdateWarSideRequest warSideRequest)
        {
            await _warSideService
                .UpdateAsync(id, warSideRequest);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _warSideService.DeleteAsync(id);
            return Ok();
        }
    }
}
