using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Infrastructure.Repositories
{
    public class SideRepository : ISideRepository
    {
        private readonly PalantirDbContext _dbContext;

        public SideRepository(PalantirDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Side>> GetAllAsync() =>
            await _dbContext.sides.ToListAsync<Side>();

        public async Task<Side?> GetByIdAsync(int id) => 
            await _dbContext.sides.FirstOrDefaultAsync<Side>(s => s.side_id == id);
    }
}
