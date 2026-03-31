using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Presenatation.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarsController : ControllerBase
    {
        private readonly IWarRepository _warsRep;
        public WarsController(IWarRepository warsRep)
        {
            _warsRep = warsRep;
        }

        //[HttpGet(Name = "GetWars")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<War>>> GetAll() =>
            await _warsRep.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<War>> GetById(int id)
        {
            var war = await _warsRep.GetByIdAsync(id);
            return war is null ? 
                NotFound() : Ok(war);
        }
    }
}
