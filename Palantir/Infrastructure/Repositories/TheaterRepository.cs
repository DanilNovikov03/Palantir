using Microsoft.EntityFrameworkCore;
using Palantir.Application.Interfaces;
using Palantir.Domain.Models;
using Palantir.Infrastructure.Data;

namespace Palantir.Infrastructure.Repositories
{
    public class TheaterRepository : ITheaterRepository
    {
        private readonly PalantirDbContext _dbContext;

        public TheaterRepository(PalantirDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Theater>> GetAllAsync() =>
            await _dbContext.theaters.ToListAsync<Theater>();

        public async Task<Theater?> GetByIdAsync(int id) =>
            await _dbContext.theaters.FirstOrDefaultAsync(t => t.theater_id == id);
    }
}
