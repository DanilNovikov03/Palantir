using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;
using Palantir.Infrastructure.Repositories;

namespace Palantir.Presenatation.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ArmiesController : ControllerBase
    {
        private readonly IArmyRepository _armyRep;

        public ArmiesController(IArmyRepository armyRep)
        {
            _armyRep = armyRep;
        }

        //[HttpGet(Name = "GetArmies")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Army>>> GetAll() =>
            await _armyRep.GetAllAsync();

        //[HttpGet(Name = "GetArmy")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Army>> GetById(int id)
        {
            var army = await _armyRep.GetByIdAsync(id);
            return army is null ? 
                NotFound() : Ok(army);
        }
    }
}
