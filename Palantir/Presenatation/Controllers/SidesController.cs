namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SidesController : ControllerBase
    {
        private readonly ISideService _sideService;

        public SidesController(ISideService sideService) =>
            _sideService = sideService;


        [HttpGet("{id}")]
        public async Task<ActionResult<Side>> GetById(int id)
        {
            var side = await _sideService.GetByIdAsync(id);
            return side is null ? 
                NotFound() : Ok(side);
        }

        [HttpPost]
        public async Task<ActionResult> Add(SideRequest sideRequest)
        {
            var response = await _sideService.AddAsync(sideRequest);
            return CreatedAtAction(nameof(GetById), new { response.Id });
        }

        [HttpPut]
        public async Task<ActionResult> Update(int id, SideRequest sideRequest)
        {
            var response = await _sideService
                .UpdateAsync(id, sideRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            await _sideService.DeleteAsync(id);
            return NoContent();
        }
    }
}
