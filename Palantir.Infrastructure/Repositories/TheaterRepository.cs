namespace Palantir.Infrastructure.Repositories
{
    public class TheaterRepository : ITheaterRepository
    {
        private readonly PalantirDbContext _dbContext;

        public TheaterRepository(PalantirDbContext dbContext) =>
            _dbContext = dbContext;


        public async Task<List<Theater>> GetAllAsync() =>
            await _dbContext.theaters.ToListAsync<Theater>();

        public async Task<Theater?> GetByIdAsync(int id) =>
            await _dbContext.theaters
                .FirstOrDefaultAsync(
                    t => t.theater_id == id
                );

        public async Task<List<Theater>> GetByWarIdAsync(int warId) =>
            await _dbContext.theaters
                .Where(theater => theater.war_id == warId)
                .ToListAsync();

        public async Task AddAsync(Theater theater)
        {
            await _dbContext.theaters.AddAsync(theater);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Theater theater)
        {
            _dbContext.theaters.Update(theater);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var theater = 
                await _dbContext.theaters.FindAsync(id);

            if (theater  != null)
            {
                _dbContext.theaters.Remove(theater);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id) =>
            await _dbContext.theaters
                .AnyAsync(t => t.theater_id == id);
    }
}
