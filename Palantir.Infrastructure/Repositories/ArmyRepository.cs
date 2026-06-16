namespace Palantir.Infrastructure.Repositories
{
    public class ArmyRepository : IArmyRepository
    {
        private readonly PalantirDbContext _dbContext;

        public ArmyRepository(PalantirDbContext dbContext) =>
            _dbContext = dbContext;

        public async Task<List<Army>> GetAllAsync() =>
            await _dbContext.armies.ToListAsync<Army>();

        public async Task<Army?> GetByIdAsync(int id) =>
            await _dbContext.armies
                .FirstOrDefaultAsync(
                    a => a.army_id == id
                );

        public async Task AddAsync(Army army)
        {
            await _dbContext.armies.AddAsync(army);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Army army)
        {
            _dbContext.armies.Update(army);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var army = 
                await _dbContext.armies.FindAsync(id);

            if (army != null)
            {
                _dbContext.armies.Remove(army);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
