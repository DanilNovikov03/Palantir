
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

        public async Task<List<ControlZone>> GetByWarDateAsync(int warId, DateOnly date)
        {
            return await _dbContext.control_zones
                .Where(zone =>
                    zone.war_id == warId &&
                    zone.date_control <= date &&
                    zone.date_control == _dbContext.control_zones
                        .Where(candidate =>
                            candidate.war_id == warId &&
                            candidate.war_side_id == zone.war_side_id &&
                            candidate.date_control <= date)
                        .Max(candidate => candidate.date_control))
                .OrderBy(zone => zone.war_side_id)
                .ThenBy(zone => zone.control_id)
                .ToListAsync();
        }

        public async Task<List<ControlZone>> GetByWarSideDateAsync(int warId, int warSideId, DateOnly date) =>
            await _dbContext.control_zones
                .Where(zone =>
                    zone.war_id == warId &&
                    zone.war_side_id == warSideId &&
                    zone.date_control <= date &&
                    zone.date_control == _dbContext.control_zones
                        .Where(candidate =>
                            candidate.war_id == warId &&
                            candidate.war_side_id == warSideId &&
                            candidate.date_control <= date)
                        .Max(candidate => candidate.date_control))
                .OrderBy(zone => zone.control_id)
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
