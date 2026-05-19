namespace Palantir.Infrastructure.Repositories
{
    public class OperationRepository : IOperationRepository
    {
        private readonly PalantirDbContext _dbContext;

        public OperationRepository(PalantirDbContext dbContext) =>
            _dbContext = dbContext;


        public async Task<List<Operation>> GetAllAsync() =>
            await _dbContext.operations.ToListAsync<Operation>();

        public async Task<Operation?> GetByIdAsync(int id) =>
            await _dbContext.operations
                .FirstOrDefaultAsync(
                    o => o.operation_id == id
                );

        public async Task<List<Operation>> GetByTheaterIdAsync(int theaterId) =>
            await _dbContext.operations
                .Where(operation => operation.theater_id == theaterId)
                .ToListAsync();

        public async Task AddAsync(Operation operation)
        {
            await _dbContext.operations.AddAsync(operation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Operation operation)
        {
            _dbContext.operations.Update(operation);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var operation = 
                await _dbContext.operations.FindAsync(id);

            if (operation != null)
            {
                _dbContext.operations.Remove(operation);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
