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
            await _dbContext.wars
                .FirstOrDefaultAsync(
                    w => w.war_id == id
                );

        public async Task AddAsync(War war)
        {
            await _dbContext.wars.AddAsync(war);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(War war)
        {
            _dbContext.wars.Update(war);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var war = 
                await _dbContext.wars.FindAsync(id);

            if (war != null)
            {
                _dbContext.wars.Remove(war);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
