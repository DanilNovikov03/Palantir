using Microsoft.AspNetCore.Mvc;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;

namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarSidesController : ControllerBase
    {
        private readonly IWarSideRepository _warSideRep;

        public WarSidesController(IWarSideRepository warSideRep)
        {
            _warSideRep = warSideRep;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarSide>>> GetAll() =>
            await _warSideRep.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<WarSide>> GetById(int id)
        {
            var warSide = await _warSideRep.GetAllAsync();
            return warSide is null ?
                NotFound() : Ok(warSide);
        }
    }
}
