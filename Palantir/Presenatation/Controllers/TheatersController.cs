namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TheatersController : ControllerBase
    {
        private readonly ITheaterService _theaterService;

        public TheatersController(ITheaterService theaterService) =>
            _theaterService = theaterService;


        [HttpGet("{id}")]
        public async Task<ActionResult<Theater>> GetById(int id)
        {
            var theater = await _theaterService.GetByIdAsync(id);
            return theater is null ? 
                NotFound() : Ok(theater);
        }

        [HttpPost]
        public async Task<ActionResult> Add(TheaterRequest theaterRequest)
        {
            var response = await _theaterService.AddAsync(theaterRequest);
            return CreatedAtAction(nameof(GetById), new { id = response.Id });
        }

        [HttpPut]
        public async Task<ActionResult> Update(int id, TheaterRequest theaterRequest)
        {
            var response = _theaterService
                .UpdateAsync(id, theaterRequest);
            return response is null ?
                NotFound() : NoContent();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            await _theaterService.DeleteAsync(id);
            return NoContent();
        }
    }
}
