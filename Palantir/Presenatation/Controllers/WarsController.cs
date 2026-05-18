namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("api/war")]
    public class WarsController : ControllerBase
    {
        private readonly IWarService _warService;

        public WarsController(IWarService warService) =>
            _warService = warService;


        //[HttpGet(Name = "GetWars")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarResponse>>> GetAll()
        {
            var wars = await _warService.GetAllAsync();
            return Ok(wars);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WarResponse>> GetById(int id)
        {
            var war = await _warService.GetByIdAsync(id);
            return war is null ? 
                NotFound() : Ok(war);
        }

        [HttpPost]
        public async Task<ActionResult> Add(WarRequest warRequest)
        {
            var response = await _warService.AddAsync(warRequest);
            return CreatedAtAction(
                nameof(GetById), 
                new { id = response.Id }, 
                response
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, WarRequest warRequest)
        {
            var response = await _warService
                .UpdateAsync(id, warRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _warService.DeleteAsync(id);
            return NoContent();
        }
    }
}
