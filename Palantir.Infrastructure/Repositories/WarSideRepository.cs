namespace Palantir.Infrastructure.Repositories
{
    public class WarSideRepository : IWarSideRepository
    {
        private readonly PalantirDbContext _dbContext;

        public WarSideRepository(PalantirDbContext dbContext) =>
            _dbContext = dbContext;

        public async Task<List<WarSide>> GetAllAsync() => 
            await _dbContext.war_sides.ToListAsync<WarSide>();

        public async Task<WarSide?> GetByIdAsync(int id) =>
            await _dbContext.war_sides
                .Include(warSide => warSide.side)
                .FirstOrDefaultAsync(
                    w => w.war_side_id == id
                );

        public async Task<WarSide?> GetByWarAndSideAsync(int warId, int sideId) =>
            await _dbContext.war_sides
                .Include(warSide => warSide.side)
                .FirstOrDefaultAsync(warSide =>
                    warSide.war_id == warId && warSide.side_id == sideId);

        public async Task<List<WarSide>> GetByWarIdAsync(int warId) =>
            await _dbContext.war_sides
                .Where(w => w.war_id == warId)
                .ToListAsync();

        public async Task<List<WarSide>> GetBySideIdAsync(int sideId) =>
            await _dbContext.war_sides
                .Where(s => s.side_id == sideId)
                .ToListAsync();

        public async Task<bool> IsUsedByOperationsAsync(int warSideId) =>
            await _dbContext.operation_sides
                .AnyAsync(operationSide => operationSide.war_side_id == warSideId);

        public async Task AddAsync(WarSide warSide)
        {
            await _dbContext.war_sides.AddAsync(warSide);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(WarSide warSide)
        {
            _dbContext.war_sides.Update(warSide);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var warSide =
                await _dbContext.war_sides.FindAsync(id);

            if (warSide != null)
            {
                _dbContext.war_sides.Remove(warSide);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
