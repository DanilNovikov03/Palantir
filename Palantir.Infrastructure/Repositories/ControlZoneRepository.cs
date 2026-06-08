
namespace Palantir.Infrastructure.Repositories
{
    public class ControlZoneRepository : IControlZoneRepository
    {
        private readonly PalantirDbContext _dbContext;

        public ControlZoneRepository(PalantirDbContext dbContext) =>
            _dbContext = dbContext;

        public async Task<ControlZone?> GetByIdAsync(int id) =>
            await _dbContext.control_zones
                .FirstOrDefaultAsync(zones => zones.control_id == id);

        public async Task<List<ControlZone>> GetByWarDateAsync(int warId, DateOnly date) =>
            await _dbContext.control_zones
                .Where(zone => 
                    zone.war_id == warId &&
                    zone.date_control == date)
                .ToListAsync();

        public async Task<List<ControlZone>> GetByWarSideDateAsync(int warId, int warSideId, DateOnly date) =>
            await _dbContext.control_zones
                .Where(zone =>
                    zone.war_id == warId &&
                    zone.war_side_id == warSideId &&
                    zone.date_control == date)
                .ToListAsync();

        public async Task AddAsync(ControlZone controlZone)
        {
            await _dbContext
                .control_zones.AddAsync(controlZone);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(ControlZone controlZone)
        {
            _dbContext.control_zones.Update(controlZone);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var zone =
                await _dbContext.control_zones.FindAsync(id);

            if (zone != null)
            {
                _dbContext.control_zones.Remove(zone);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
