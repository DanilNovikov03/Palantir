using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Infrastructure.Repositories
{
    public class ArmyRepository : IArmyRepository
    {
        private readonly PalantirDbContext _dbContext;

        public ArmyRepository(PalantirDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Army>> GetAllAsync() =>
            await _dbContext.armies.ToListAsync<Army>();

        public async Task<Army?> GetByIdAsync(int id) =>
            await _dbContext.armies.FirstOrDefaultAsync(a => a.army_id == id);
    }
}
