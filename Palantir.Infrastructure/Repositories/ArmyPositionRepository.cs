namespace Palantir.Infrastructure.Repositories
{
    public class ArmyPositionRepository : IArmyPositionRepository
    {
        private readonly PalantirDbContext _dbContext;

        public ArmyPositionRepository(PalantirDbContext dbContext) =>
            _dbContext = dbContext;

        public async Task<List<ArmyPosition>> GetAllAsync() =>
            await _dbContext.army_positions
                .ToListAsync<ArmyPosition>();

        public async Task<ArmyPosition?> GetByIdAsync(int id) =>
            await _dbContext.army_positions
                .FirstOrDefaultAsync<ArmyPosition>(
                    ap => ap.army_position_id == id
                );

        public async Task AddAsync(ArmyPosition position)
        {
            await _dbContext.army_positions.AddAsync(position);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(ArmyPosition position)
        {
            _dbContext.army_positions.Update(position);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var position =
                await _dbContext.army_positions.FindAsync(id);

            if (position != null)
            {
                _dbContext.army_positions.Remove(position);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
