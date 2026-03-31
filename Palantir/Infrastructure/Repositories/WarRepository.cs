using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Infrastructure.Repositories
{
    public class WarRepository : IWarRepository
    {
        private readonly PalantirDbContext _dbContext;

        public WarRepository(PalantirDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<War>> GetAllAsync() =>
            await _dbContext.wars.ToListAsync<War>();

        public async Task<War?> GetByIdAsync(int id) =>
            await _dbContext.wars.FirstOrDefaultAsync(w => w.war_id == id);
    }
}
