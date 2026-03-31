using Microsoft.AspNetCore.Mvc;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;

namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TheatersController : ControllerBase
    {
        private readonly ITheaterRepository _theaterRep;

        public TheatersController(ITheaterRepository theaterRep)
        {
            _theaterRep = theaterRep;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Theater>>> GetAll() =>
            await _theaterRep.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Theater>> GetById(int id)
        {
            var theater = await _theaterRep.GetByIdAsync(id);
            return theater is null ? 
                NotFound() : Ok(theater);
        }
    }
}
