namespace Palantir.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly PalantirDbContext _dbContext;

        public EventRepository(PalantirDbContext dbContext) =>
            _dbContext = dbContext;

        public async Task<Event?> GetByIdAsync(int id) =>
            await EventsWithSide()
                .FirstOrDefaultAsync(mapEvent => mapEvent.event_id == id);

        public async Task<List<Event>> GetByOperationAsync(int operationId) =>
            await EventsWithSide()
                .Where(mapEvent => mapEvent.operation_id == operationId)
                .OrderBy(mapEvent => mapEvent.date_ev)
                .ThenBy(mapEvent => mapEvent.event_id)
                .ToListAsync();

        public async Task<List<Event>> GetByWarDateAsync(int warId, DateOnly date, int? operationId)
        {
            var query = EventsWithSide()
                .Where(mapEvent =>
                    mapEvent.war_id == warId &&
                    mapEvent.date_ev == date);

            if (operationId is not null)
            {
                query = query.Where(mapEvent => mapEvent.operation_id == operationId);
            }

            return await query
                .OrderBy(mapEvent => mapEvent.event_id)
                .ToListAsync();
        }

        public async Task AddAsync(Event mapEvent)
        {
            await _dbContext.events.AddAsync(mapEvent);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Event mapEvent)
        {
            _dbContext.events.Update(mapEvent);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var mapEvent = await _dbContext.events.FindAsync(id);

            if (mapEvent is null)
            {
                return;
            }

            _dbContext.events.Remove(mapEvent);
            await _dbContext.SaveChangesAsync();
        }

        private IQueryable<Event> EventsWithSide() =>
            _dbContext.events
                .Include(mapEvent => mapEvent.war_side)
                    .ThenInclude(warSide => warSide!.side);
    }
}
