using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArmiesController : ControllerBase
    {
        private readonly PalantirDbContext _context;
        private readonly ILogger<ArmiesController> _logger;

        public ArmiesController(ILogger<ArmiesController> logger, PalantirDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetArmies")]
        public async Task<ActionResult<IEnumerable<Army>>> GetArmies()
            => await _context.armies.ToListAsync();
    }
}
