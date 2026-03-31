using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Infrastructure.Repositories
{
    public class WarSideRepository : IWarSideRepository
    {
        private readonly PalantirDbContext _dbContext;

        public WarSideRepository(PalantirDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<WarSide>> GetAllAsync() => 
            await _dbContext.war_sides.ToListAsync<WarSide>();
    }
}
