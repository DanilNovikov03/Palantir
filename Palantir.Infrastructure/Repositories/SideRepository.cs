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
            await _dbContext.sides
                .FirstOrDefaultAsync<Side>(
                    s => s.side_id == id
                );

        public async Task AddAsync(Side side)
        {
            await _dbContext.sides.AddAsync(side);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Side side)
        {
            _dbContext.sides.Update(side);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var side = await _dbContext.sides.FindAsync(id);

            if (side != null)
            {
                _dbContext.sides.Remove(side);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
