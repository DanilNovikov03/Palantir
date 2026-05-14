namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SidesController : ControllerBase
    {
        private readonly ISideRepository _sideRep;

        public SidesController(ISideRepository sideRep)
        {
            _sideRep = sideRep;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Side>>> GetAll() =>
            await _sideRep.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Side>> GetById(int id)
        {
            var side = await _sideRep.GetByIdAsync(id);
            return side is null ? 
                NotFound() : Ok(side);
        }
    }
}
