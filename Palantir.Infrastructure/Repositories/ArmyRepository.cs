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

        public async Task<List<Army>> GetByWarDateWithPositionsAsync(int warId, DateOnly date) =>
            await _dbContext.armies
                .AsNoTracking()
                .Include(army => army.war_side)
                    .ThenInclude(warSide => warSide.side)
                .Include(army => army.army_positions
                    .Where(position => position.date_position <= date)
                    .OrderByDescending(position => position.date_position)
                    .Take(1))
                .Where(army =>
                    army.war_side.war_id == warId &&
                    army.army_positions.Any(position => position.date_position <= date))
                .ToListAsync();

        public async Task AddAsync(Army army)
        {
            await _dbContext.armies.AddAsync(army);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddWithPositionAsync(Army army, ArmyPosition position)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            await _dbContext.armies.AddAsync(army);
            await _dbContext.SaveChangesAsync();

            position.army_id = army.army_id;
            await _dbContext.army_positions.AddAsync(position);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        public async Task<Army?> UpsertPositionAsync(int armyId, ArmyPosition position)
        {
            var army = await _dbContext.armies
                .Include(army => army.war_side)
                    .ThenInclude(warSide => warSide.side)
                .Include(army => army.army_positions)
                .FirstOrDefaultAsync(army => army.army_id == armyId);

            if (army is null)
            {
                return null;
            }

            var existingPosition = await _dbContext.army_positions
                .FirstOrDefaultAsync(armyPosition =>
                    armyPosition.army_id == armyId &&
                    armyPosition.date_position == position.date_position);

            if (existingPosition is null)
            {
                position.army_id = armyId;
                await _dbContext.army_positions.AddAsync(position);
                army.army_positions.Add(position);
            }
            else
            {
                existingPosition.coordinate = position.coordinate;
                existingPosition.note = position.note;
            }

            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(army)
                .Collection(currentArmy => currentArmy.army_positions)
                .Query()
                .Where(armyPosition => armyPosition.date_position == position.date_position)
                .LoadAsync();

            return army;
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
                var positions = await _dbContext.army_positions
                    .Where(position => position.army_id == id)
                    .ToListAsync();

                _dbContext.army_positions.RemoveRange(positions);
                _dbContext.armies.Remove(army);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
