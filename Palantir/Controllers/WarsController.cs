using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarsController : ControllerBase
    {
        private readonly PalantirDbContext _context;

        private readonly ILogger<WarsController> _logger;

        public WarsController(ILogger<WarsController> logger, PalantirDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet(Name = "GetWars")]
        public async Task<ActionResult<IEnumerable<War>>> GetWars() =>
            await _context.wars.ToListAsync();
    }
}
